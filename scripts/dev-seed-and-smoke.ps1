param(
    [string]$ApiBase = "http://localhost:5010/api/v1",
    [string]$DbContainer = "MedHub.Db",
    [string]$DbUser = "postgres",
    [string]$DbName = "medhub",
    [string]$TeacherEmail = "teacher@mail.ru",
    [string]$TeacherPassword = "au2-Wem-rHr-UwZ",
    [string]$StudentEmail = "student-dev-smoke@mail.ru",
    [string]$StudentPassword = "stu-Wem-rHr-UwZ",
    [string]$ForeignStudentEmail = "student-foreign-smoke@mail.ru",
    [string]$ForeignStudentPassword = "stu2-Wem-rHr-UwZ"
)

$ErrorActionPreference = "Stop"

$prefix = "[DEV-SMOKE]"
$steps = New-Object System.Collections.Generic.List[object]
$created = New-Object System.Collections.Generic.List[string]
$limitations = New-Object System.Collections.Generic.List[string]

function Add-Step {
    param(
        [string]$Name,
        [string]$Method,
        [string]$Path,
        [int]$Status,
        [string]$Expected,
        [bool]$Passed,
        [string]$Details = ""
    )

    $script:steps.Add([pscustomobject]@{
        name = $Name
        method = $Method
        path = $Path
        status = $Status
        expected = $Expected
        passed = $Passed
        details = $Details
    }) | Out-Null
}

function Convert-ToJsonBody {
    param([object]$Body)

    if ($null -eq $Body) {
        return $null
    }

    return ($Body | ConvertTo-Json -Depth 20)
}

function Invoke-Api {
    param(
        [string]$Method,
        [string]$Path,
        [string]$Token = "",
        [object]$Body = $null,
        [switch]$Raw
    )

    $uri = if ($Path.StartsWith("http", [StringComparison]::OrdinalIgnoreCase)) { $Path } else { "$ApiBase$Path" }
    $headers = @{}

    if ($Token) {
        $headers.Authorization = "Bearer $Token"
    }

    $json = Convert-ToJsonBody $Body

    try {
        $response = if ($null -ne $json) {
            Invoke-WebRequest -UseBasicParsing -Method $Method -Uri $uri -Headers $headers -ContentType "application/json" -Body $json
        } else {
            Invoke-WebRequest -UseBasicParsing -Method $Method -Uri $uri -Headers $headers
        }

        $contentText = if ($response.Content -is [byte[]]) {
            [System.Text.Encoding]::UTF8.GetString($response.Content)
        } else {
            [string]$response.Content
        }

        $data = $null
        if (-not $Raw -and $contentText) {
            try {
                $data = $contentText | ConvertFrom-Json
            } catch {
                $data = $contentText
            }
        }

        return [pscustomobject]@{
            status = [int]$response.StatusCode
            data = $data
            text = $contentText
            headers = $response.Headers
            ok = $true
            uri = $uri
        }
    } catch {
        $statusCode = 0
        $text = $_.Exception.Message

        if ($_.Exception.Response) {
            $statusCode = [int]$_.Exception.Response.StatusCode
            $stream = $_.Exception.Response.GetResponseStream()
            if ($stream) {
                $reader = New-Object System.IO.StreamReader($stream)
                $bodyText = $reader.ReadToEnd()
                if ($bodyText) {
                    $text = $bodyText
                }
            }
        }

        $data = $null
        if (-not $Raw -and $text) {
            try {
                $data = $text | ConvertFrom-Json
            } catch {
                $data = $text
            }
        }

        return [pscustomobject]@{
            status = $statusCode
            data = $data
            text = $text
            headers = @{}
            ok = $false
            uri = $uri
        }
    }
}

function Sql-Literal {
    param([string]$Value)
    return "'" + ($Value -replace "'", "''") + "'"
}

function Invoke-DevSql {
    param([string]$Sql)

    $output = $Sql | & docker exec -i $DbContainer psql -U $DbUser -d $DbName -At -F "|" -v ON_ERROR_STOP=1
    if ($LASTEXITCODE -ne 0) {
        throw "SQL failed: $Sql"
    }

    return (($output | Out-String).Trim())
}

function Ensure-Role {
    param(
        [string]$Email,
        [int]$RoleId
    )

    $emailSql = Sql-Literal $Email
    Invoke-DevSql "insert into role_user (roles_id, users_id) select $RoleId, id from users where email = $emailSql on conflict do nothing;" | Out-Null
}

function Ensure-User {
    param(
        [string]$Email,
        [string]$Password,
        [string]$FirstName,
        [string]$LastName,
        [int]$RoleId
    )

    $login = Invoke-Api -Method "POST" -Path "/users/login" -Body @{ email = $Email; password = $Password }

    if ($login.status -eq 200) {
        Ensure-Role -Email $Email -RoleId $RoleId
        return $login.data.accessToken
    }

    $register = Invoke-Api -Method "POST" -Path "/users/register" -Body @{
        email = $Email
        firstName = $FirstName
        lastName = $LastName
        password = $Password
    }

    if ($register.status -eq 200) {
        $script:created.Add("user $Email") | Out-Null
        Ensure-Role -Email $Email -RoleId $RoleId
        Start-Sleep -Milliseconds 300
        $login = Invoke-Api -Method "POST" -Path "/users/login" -Body @{ email = $Email; password = $Password }
    } else {
        Ensure-Role -Email $Email -RoleId $RoleId
        $login = Invoke-Api -Method "POST" -Path "/users/login" -Body @{ email = $Email; password = $Password }
    }

    if ($login.status -ne 200) {
        throw "Cannot login $Email. Register status: $($register.status). Login status: $($login.status). $($login.text)"
    }

    return $login.data.accessToken
}

function Get-Courses {
    param([string]$Token)

    $response = Invoke-Api -Method "GET" -Path "/courses" -Token $Token
    if ($response.status -ne 200) {
        throw "GET /courses failed with $($response.status): $($response.text)"
    }

    return @($response.data)
}

function Ensure-Course {
    param(
        [string]$Token,
        [string]$Title,
        [string]$Description
    )

    $course = Get-Courses -Token $Token | Where-Object { $_.title -eq $Title } | Select-Object -First 1

    if ($course) {
        return $course.id
    }

    $response = Invoke-Api -Method "POST" -Path "/courses" -Token $Token -Body @{
        title = $Title
        description = $Description
    }

    if ($response.status -ne 200) {
        throw "POST /courses failed with $($response.status): $($response.text)"
    }

    $script:created.Add("course $Title") | Out-Null
    return [string]$response.data
}

function Get-CourseContent {
    param(
        [string]$Token,
        [string]$CourseId
    )

    $response = Invoke-Api -Method "GET" -Path "/courses/$CourseId/content" -Token $Token
    if ($response.status -ne 200) {
        throw "GET /courses/$CourseId/content failed with $($response.status): $($response.text)"
    }

    return @($response.data)
}

function Ensure-Lesson {
    param(
        [string]$Token,
        [string]$CourseId,
        [string]$Title,
        [int]$Order,
        [int]$ContentType,
        [string]$ContentUrl
    )

    $lesson = Get-CourseContent -Token $Token -CourseId $CourseId |
        Where-Object { $_.title -eq $Title } |
        Select-Object -First 1

    if ($lesson) {
        return $lesson.id
    }

    $response = Invoke-Api -Method "POST" -Path "/lessons" -Token $Token -Body @{
        courseId = $CourseId
        title = $Title
        order = $Order
        contentType = $ContentType
        contentUrl = $ContentUrl
    }

    if ($response.status -ne 200) {
        throw "POST /lessons failed with $($response.status): $($response.text)"
    }

    $script:created.Add("lesson $Title") | Out-Null
    return [string]$response.data
}

function Ensure-DraftRuntimeBlockedLesson {
    param(
        [string]$Token,
        [string]$CourseId
    )

    $title = "$prefix Runtime blocked draft lesson"
    $lesson = Get-CourseContent -Token $Token -CourseId $CourseId |
        Where-Object {
            $_.title -eq $title -and
            ([string]$_.status).ToLowerInvariant() -ne "published" -and
            -not $_.videoId
        } |
        Select-Object -First 1

    if ($lesson) {
        return $lesson.id
    }

    $order = Get-Random -Minimum 1000 -Maximum 9999
    $response = Invoke-Api -Method "POST" -Path "/lessons" -Token $Token -Body @{
        courseId = $CourseId
        title = $title
        order = $order
        contentType = 2
        contentUrl = "https://medhub.local/dev/runtime/blocked"
    }

    if ($response.status -ne 200) {
        throw "POST /lessons for blocked runtime smoke failed with $($response.status): $($response.text)"
    }

    $script:created.Add("draft runtime-blocked lesson $title") | Out-Null
    return [string]$response.data
}

function Get-Lesson {
    param(
        [string]$Token,
        [string]$LessonId
    )

    $response = Invoke-Api -Method "GET" -Path "/lessons/$LessonId" -Token $Token
    if ($response.status -ne 200) {
        throw "GET /lessons/$LessonId failed with $($response.status): $($response.text)"
    }

    return $response.data
}

function Invoke-Expected {
    param(
        [string]$Name,
        [string]$Method,
        [string]$Path,
        [string]$Token,
        [object]$Body = $null,
        [int[]]$ExpectedStatus
    )

    $response = Invoke-Api -Method $Method -Path $Path -Token $Token -Body $Body
    $passed = $ExpectedStatus -contains $response.status
    Add-Step -Name $Name -Method $Method -Path $Path -Status $response.status -Expected ($ExpectedStatus -join "/") -Passed $passed -Details $response.text
    return $response
}

function Ensure-ReadyVideo {
    param([string]$TeacherEmail)

    $emailSql = Sql-Literal $TeacherEmail
    $sql = @"
select
    v.id,
    v.lesson_id,
    l.course_id,
    coalesce(v.duration_seconds, 0),
    coalesce(v.storage_key, '')
from videos v
join lessons l on l.id = v.lesson_id
join courses c on c.id = l.course_id
join users u on u.id = c.creator_id
where u.email = $emailSql
  and v.status = 'Ready'
  and l.video_id = v.id
order by coalesce(v.duration_seconds, 0) desc, v.created_at desc
limit 1;
"@

    $line = Invoke-DevSql $sql
    if (-not $line) {
        return $null
    }

    $parts = $line -split "\|"
    return [pscustomobject]@{
        videoId = $parts[0]
        lessonId = $parts[1]
        courseId = $parts[2]
        durationSeconds = [int]$parts[3]
        storageKey = $parts[4]
    }
}

function Get-VideoCheckpoints {
    param(
        [string]$Token,
        [string]$VideoId
    )

    $response = Invoke-Api -Method "GET" -Path "/checkpoints/video/$VideoId" -Token $Token
    if ($response.status -ne 200) {
        throw "GET /checkpoints/video/$VideoId failed with $($response.status): $($response.text)"
    }

    return @($response.data)
}

function Get-Checkpoint {
    param(
        [string]$Token,
        [string]$CheckpointId
    )

    $response = Invoke-Api -Method "GET" -Path "/checkpoints/$CheckpointId" -Token $Token
    if ($response.status -ne 200) {
        throw "GET /checkpoints/$CheckpointId failed with $($response.status): $($response.text)"
    }

    return $response.data
}

function Get-UsedCheckpointNumbers {
    param(
        [string]$Token,
        [string]$VideoId
    )

    $items = Get-VideoCheckpoints -Token $Token -VideoId $VideoId
    return [pscustomobject]@{
        timestamps = @($items | ForEach-Object { [int]$_.timestampSeconds })
        orders = @($items | ForEach-Object { [int]$_.orderNumber })
    }
}

function Get-FreeCheckpointSlot {
    param(
        [string]$Token,
        [string]$VideoId,
        [int]$DurationSeconds
    )

    $used = Get-UsedCheckpointNumbers -Token $Token -VideoId $VideoId
    $maxTimestamp = [Math]::Max(1, $DurationSeconds - 1)
    $order = 1
    while ($used.orders -contains $order) { $order++ }

    $timestamp = 1
    while (($used.timestamps -contains $timestamp) -and ($timestamp -lt $maxTimestamp)) { $timestamp++ }

    if ($used.timestamps -contains $timestamp) {
        throw "No free checkpoint timestamp for video $VideoId"
    }

    return [pscustomobject]@{
        timestamp = $timestamp
        order = $order
    }
}

function Ensure-Checkpoint {
    param(
        [string]$Token,
        [string]$VideoId,
        [int]$DurationSeconds,
        [string]$Title,
        [bool]$IsRequired,
        [bool]$IsGraded
    )

    $checkpoint = Get-VideoCheckpoints -Token $Token -VideoId $VideoId |
        Where-Object { $_.title -eq $Title } |
        Select-Object -First 1

    if ($checkpoint) {
        return $checkpoint.id
    }

    $slot = Get-FreeCheckpointSlot -Token $Token -VideoId $VideoId -DurationSeconds $DurationSeconds
    $response = Invoke-Api -Method "POST" -Path "/checkpoints" -Token $Token -Body @{
        videoId = $VideoId
        timestampSeconds = $slot.timestamp
        orderNumber = $slot.order
        title = $Title
        isRequired = $IsRequired
        isGraded = $IsGraded
    }

    if ($response.status -ne 200) {
        throw "POST /checkpoints failed with $($response.status): $($response.text)"
    }

    $script:created.Add("checkpoint $Title") | Out-Null
    return [string]$response.data
}

function Ensure-QuestionBySql {
    param(
        [string]$CheckpointId,
        [string]$Text
    )

    $checkpointSql = Sql-Literal $CheckpointId
    $textSql = Sql-Literal $Text
    $questionId = Invoke-DevSql "select id from checkpoint_questions where checkpoint_id = $checkpointSql::uuid and text = $textSql limit 1;"

    if (-not $questionId) {
        $questionId = [guid]::NewGuid().ToString()
        $idSql = Sql-Literal $questionId
        Invoke-DevSql @"
insert into checkpoint_questions
    (id, checkpoint_id, text, type, allow_retry, time_limit_seconds, reveal_correct_answer, correct_text_answer)
values
    ($idSql::uuid, $checkpointSql::uuid, $textSql, 'SingleChoice', true, null, true, null);
"@ | Out-Null
        $script:created.Add("SingleChoice question for checkpoint $CheckpointId") | Out-Null
    }

    $options = @(
        @{ text = "Systole"; correct = $false },
        @{ text = "Diastole"; correct = $true },
        @{ text = "Apnea"; correct = $false }
    )

    foreach ($option in $options) {
        $optionTextSql = Sql-Literal $option.text
        $isCorrect = if ($option.correct) { "true" } else { "false" }
        $exists = Invoke-DevSql "select id from checkpoint_answer_options where question_id = $(Sql-Literal $questionId)::uuid and text = $optionTextSql limit 1;"

        if (-not $exists) {
            $optionId = [guid]::NewGuid().ToString()
            Invoke-DevSql @"
insert into checkpoint_answer_options (id, question_id, text, is_correct)
values ($(Sql-Literal $optionId)::uuid, $(Sql-Literal $questionId)::uuid, $optionTextSql, $isCorrect);
"@ | Out-Null
            $script:created.Add("answer option '$($option.text)'") | Out-Null
        }
    }

    return $questionId
}

function Ensure-Question {
    param(
        [string]$Token,
        [string]$CheckpointId
    )

    $detail = Get-Checkpoint -Token $Token -CheckpointId $CheckpointId
    if (@($detail.questions).Count -gt 0) {
        return @($detail.questions)[0].id
    }

    $questionBody = @{
        text = "Which heart phase fills the ventricles?"
        type = 1
        allowRetry = $true
        timeLimitSeconds = $null
        revealCorrectAnswer = $true
        correctTextAnswer = $null
    }

    $response = Invoke-Api -Method "POST" -Path "/checkpoints/$CheckpointId/questions" -Token $Token -Body $questionBody
    if ($response.status -eq 200) {
        $questionId = [string]$response.data
        foreach ($option in @(
            @{ text = "Systole"; isCorrect = $false },
            @{ text = "Diastole"; isCorrect = $true },
            @{ text = "Apnea"; isCorrect = $false }
        )) {
            $optionResponse = Invoke-Api -Method "POST" -Path "/questions/$questionId/options" -Token $Token -Body $option
            if ($optionResponse.status -ne 200) {
                throw "POST /questions/$questionId/options failed with $($optionResponse.status): $($optionResponse.text)"
            }
        }

        $script:created.Add("question through API for checkpoint $CheckpointId") | Out-Null
        return $questionId
    }

    $script:limitations.Add("Question write API returned $($response.status), fallback SQL was used for dev seed data.") | Out-Null
    return Ensure-QuestionBySql -CheckpointId $CheckpointId -Text "Which heart phase fills the ventricles?"
}

function Ensure-CheckpointStatus {
    param(
        [string]$Token,
        [string]$CheckpointId,
        [string]$VideoId,
        [string]$TargetStatus
    )

    $checkpoint = Get-Checkpoint -Token $Token -CheckpointId $CheckpointId
    $status = ([string]$checkpoint.status).ToLowerInvariant()

    if ($TargetStatus -eq "Published") {
        if ($status -ne "published") {
            $response = Invoke-Api -Method "POST" -Path "/checkpoints/$CheckpointId/publish" -Token $Token
            if ($response.status -ne 204) {
                throw "Publish checkpoint $CheckpointId failed with $($response.status): $($response.text)"
            }
        }
        return
    }

    if ($TargetStatus -eq "Archived") {
        if ($status -eq "draft") {
            $publish = Invoke-Api -Method "POST" -Path "/checkpoints/$CheckpointId/publish" -Token $Token
            if ($publish.status -ne 204) {
                throw "Publish before archive checkpoint $CheckpointId failed with $($publish.status): $($publish.text)"
            }
        }

        $checkpoint = Get-Checkpoint -Token $Token -CheckpointId $CheckpointId
        $status = ([string]$checkpoint.status).ToLowerInvariant()
        if ($status -ne "archived") {
            $archive = Invoke-Api -Method "POST" -Path "/checkpoints/$CheckpointId/archive" -Token $Token
            if ($archive.status -ne 204) {
                throw "Archive checkpoint $CheckpointId failed with $($archive.status): $($archive.text)"
            }
        }
    }
}

function Ensure-LessonPublished {
    param(
        [string]$Token,
        [string]$LessonId
    )

    $lesson = Get-Lesson -Token $Token -LessonId $LessonId
    $status = ([string]$lesson.status).ToLowerInvariant()

    if ($status -ne "published") {
        $response = Invoke-Api -Method "POST" -Path "/lessons/$LessonId/publish" -Token $Token
        if ($response.status -ne 204) {
            throw "Publish lesson $LessonId failed with $($response.status): $($response.text)"
        }
    }
}

function Ensure-CoursePublished {
    param(
        [string]$Token,
        [string]$CourseId
    )

    $course = Invoke-Api -Method "GET" -Path "/courses/$CourseId" -Token $Token
    if ($course.status -ne 200) {
        throw "GET /courses/$CourseId failed with $($course.status): $($course.text)"
    }

    $status = ([string]$course.data.status).ToLowerInvariant()
    if ($status -ne "published") {
        $response = Invoke-Api -Method "POST" -Path "/courses/$CourseId/publish" -Token $Token
        if ($response.status -ne 204) {
            throw "Publish course $CourseId failed with $($response.status): $($response.text)"
        }
    }
}

function Get-CorrectOptionIdBySql {
    param([string]$QuestionId)

    $questionSql = Sql-Literal $QuestionId
    return Invoke-DevSql "select id from checkpoint_answer_options where question_id = $questionSql::uuid and is_correct = true limit 1;"
}

function Get-AttemptAnswerCountBySql {
    param(
        [string]$AttemptId,
        [string]$QuestionId
    )

    $attemptSql = Sql-Literal $AttemptId
    $questionSql = Sql-Literal $QuestionId
    $count = Invoke-DevSql "select count(*) from attempt_answers where attempt_id = $attemptSql::uuid and question_id = $questionSql::uuid;"
    return [int]$count
}

function Clear-EnrollmentByEmailAndCourse {
    param(
        [string]$Email,
        [string]$CourseId
    )

    $emailSql = Sql-Literal $Email
    $courseSql = Sql-Literal $CourseId

    Invoke-DevSql @"
delete from enrollments e
using users u
where e.student_id = u.id
  and u.email = $emailSql
  and e.course_id = $courseSql::uuid;
"@ | Out-Null
}

function Get-UnpublishedCourseId {
    param([string]$TeacherEmail)

    $emailSql = Sql-Literal $TeacherEmail

    return Invoke-DevSql @"
select c.id
from courses c
join users u on u.id = c.creator_id
where u.email = $emailSql
  and c.status <> 'Published'
limit 1;
"@
}

function Get-QuestionFromAnotherLessonBySql {
    param(
        [string]$QuestionId,
        [string]$LessonId
    )

    $questionSql = Sql-Literal $QuestionId
    $lessonSql = Sql-Literal $LessonId

    return Invoke-DevSql @"
select q.id
from checkpoint_questions q
join checkpoints cp on cp.id = q.checkpoint_id
join videos v on v.id = cp.video_id
where q.id <> $questionSql::uuid
  and v.lesson_id <> $lessonSql::uuid
limit 1;
"@
}

function Assert-CatalogCoursesPublished {
    param([object[]]$Courses)

    foreach ($course in $Courses) {
        $courseIdSql = Sql-Literal ([string]$course.id)
        $status = Invoke-DevSql "select status from courses where id = $courseIdSql::uuid;"

        if ($status -ne "Published") {
            throw "Catalog returned non-published course $($course.id) with status $status"
        }

        if ([int]$course.lessonsCount -lt [int]$course.publishedLessonsCount) {
            throw "Catalog course $($course.id) has lessonsCount < publishedLessonsCount"
        }
    }
}

function Assert-CatalogLessonsPublished {
    param([object[]]$Lessons)

    foreach ($lesson in $Lessons) {
        $lessonIdSql = Sql-Literal ([string]$lesson.id)
        $status = Invoke-DevSql "select status from lessons where id = $lessonIdSql::uuid;"

        if ($status -ne "Published") {
            throw "Catalog returned non-published lesson $($lesson.id) with status $status"
        }
    }
}

function Get-RootFromApiBase {
    $uri = [Uri]$ApiBase
    return "$($uri.Scheme)://$($uri.Authority)"
}

Write-Host "MedHub dev seed + smoke"
Write-Host "API: $ApiBase"
$smokeStartedAt = (Get-Date).ToUniversalTime().ToString("o")

$teacherToken = Ensure-User -Email $TeacherEmail -Password $TeacherPassword -FirstName "Teacher" -LastName "Smoke" -RoleId 2
$studentToken = Ensure-User -Email $StudentEmail -Password $StudentPassword -FirstName "Student" -LastName "Smoke" -RoleId 1
$foreignStudentToken = Ensure-User -Email $ForeignStudentEmail -Password $ForeignStudentPassword -FirstName "Foreign" -LastName "Student" -RoleId 1

Add-Step -Name "teacher login" -Method "POST" -Path "/users/login" -Status 200 -Expected "200" -Passed $true

$registeredTeacherEmail = "teacher-registered-smoke-$([guid]::NewGuid().ToString('N').Substring(0, 8))@mail.ru"
$registeredTeacherPassword = "teacher-reg-Wem-rHr-UwZ"

Invoke-Expected -Name "teacher registration rejects invalid code" -Method "POST" -Path "/users/register-teacher" -ExpectedStatus @(400) -Body @{
    email = "teacher-invalid-$([guid]::NewGuid().ToString('N').Substring(0, 8))@mail.ru"
    firstName = "Invalid"
    lastName = "Teacher"
    password = $registeredTeacherPassword
    teacherRegistrationCode = "wrong-code"
} | Out-Null

$registeredTeacher = Invoke-Expected -Name "teacher registers with invite code" -Method "POST" -Path "/users/register-teacher" -ExpectedStatus @(200) -Body @{
    email = $registeredTeacherEmail
    firstName = "Registered"
    lastName = "Teacher"
    password = $registeredTeacherPassword
    teacherRegistrationCode = "teacher-demo"
}

if ($registeredTeacher.status -eq 200) {
    $registeredTeacherLogin = Invoke-Expected -Name "registered teacher login" -Method "POST" -Path "/users/login" -ExpectedStatus @(200) -Body @{
        email = $registeredTeacherEmail
        password = $registeredTeacherPassword
    }

    if ($registeredTeacherLogin.status -eq 200) {
        Invoke-Expected -Name "registered teacher can access courses" -Method "GET" -Path "/courses" -Token $registeredTeacherLogin.data.accessToken -ExpectedStatus @(200) | Out-Null
    }
}

$course1 = Ensure-Course -Token $teacherToken -Title "$prefix Anatomy Foundations" -Description "Human anatomy basics for first-year students."
$course2 = Ensure-Course -Token $teacherToken -Title "$prefix Cardiology Practice" -Description "Clinical cardiology practice with checkpoints."
$course3 = Ensure-Course -Token $teacherToken -Title "$prefix Video Checkpoints Lab" -Description "Authoring lab for video checkpoints."

$lesson1 = Ensure-Lesson -Token $teacherToken -CourseId $course1 -Title "$prefix Bones and joints" -Order 1 -ContentType 2 -ContentUrl "https://medhub.local/dev/anatomy/bones"
$lesson2 = Ensure-Lesson -Token $teacherToken -CourseId $course1 -Title "$prefix Muscles overview" -Order 2 -ContentType 2 -ContentUrl "https://medhub.local/dev/anatomy/muscles"
$lesson3 = Ensure-Lesson -Token $teacherToken -CourseId $course2 -Title "$prefix Cardiac cycle" -Order 1 -ContentType 2 -ContentUrl "https://medhub.local/dev/cardio/cardiac-cycle"
$lesson4 = Ensure-Lesson -Token $teacherToken -CourseId $course3 -Title "$prefix Checkpoint authoring warmup" -Order 1 -ContentType 2 -ContentUrl "https://medhub.local/dev/video/warmup"
$blockedRuntimeLesson = Ensure-DraftRuntimeBlockedLesson -Token $teacherToken -CourseId $course3

Ensure-LessonPublished -Token $teacherToken -LessonId $lesson1
Ensure-LessonPublished -Token $teacherToken -LessonId $lesson2
Ensure-LessonPublished -Token $teacherToken -LessonId $lesson3

$readyVideo = Ensure-ReadyVideo -TeacherEmail $TeacherEmail
if (-not $readyVideo) {
    $limitations.Add("No existing Ready video attached to a teacher lesson; HLS and checkpoint smoke were skipped.") | Out-Null
} else {
    $created.Add("reused Ready video $($readyVideo.videoId) from lesson $($readyVideo.lessonId)") | Out-Null
    Ensure-LessonPublished -Token $teacherToken -LessonId $readyVideo.lessonId
    Ensure-CoursePublished -Token $teacherToken -CourseId $readyVideo.courseId

    $practicePublished = Ensure-Checkpoint -Token $teacherToken -VideoId $readyVideo.videoId -DurationSeconds $readyVideo.durationSeconds -Title "$prefix Practice published" -IsRequired $true -IsGraded $false
    Ensure-CheckpointStatus -Token $teacherToken -CheckpointId $practicePublished -VideoId $readyVideo.videoId -TargetStatus "Published"

    $gradedDraft = Ensure-Checkpoint -Token $teacherToken -VideoId $readyVideo.videoId -DurationSeconds $readyVideo.durationSeconds -Title "$prefix Graded draft no question" -IsRequired $true -IsGraded $true

    $gradedPublished = Ensure-Checkpoint -Token $teacherToken -VideoId $readyVideo.videoId -DurationSeconds $readyVideo.durationSeconds -Title "$prefix Graded SingleChoice published" -IsRequired $true -IsGraded $true
    $questionId = Ensure-Question -Token $teacherToken -CheckpointId $gradedPublished
    Ensure-CheckpointStatus -Token $teacherToken -CheckpointId $gradedPublished -VideoId $readyVideo.videoId -TargetStatus "Published"

    $archivedPractice = Ensure-Checkpoint -Token $teacherToken -VideoId $readyVideo.videoId -DurationSeconds $readyVideo.durationSeconds -Title "$prefix Practice archived" -IsRequired $false -IsGraded $false
    Ensure-CheckpointStatus -Token $teacherToken -CheckpointId $archivedPractice -VideoId $readyVideo.videoId -TargetStatus "Archived"
}

$courses = Invoke-Expected -Name "teacher sees courses" -Method "GET" -Path "/courses" -Token $teacherToken -ExpectedStatus @(200)
$hasSeedCourse = @($courses.data | Where-Object { $_.title -eq "$prefix Anatomy Foundations" }).Count -gt 0
if (-not $hasSeedCourse) {
    ($steps[$steps.Count - 1]).passed = $false
    ($steps[$steps.Count - 1]).details = "Seed course not present in list."
}

Invoke-Expected -Name "teacher opens course" -Method "GET" -Path "/courses/$course1" -Token $teacherToken -ExpectedStatus @(200) | Out-Null
Invoke-Expected -Name "teacher opens lesson" -Method "GET" -Path "/lessons/$lesson1" -Token $teacherToken -ExpectedStatus @(200) | Out-Null

if ($readyVideo) {
    $master = Invoke-Expected -Name "HLS master.m3u8" -Method "GET" -Path "/media/videos/$($readyVideo.videoId)/hls/master.m3u8" -Token $teacherToken -ExpectedStatus @(200)
    $segmentLine = ($master.text -split "`n" | ForEach-Object { $_.Trim() } | Where-Object { $_ -and -not $_.StartsWith("#") } | Select-Object -First 1)

    if ($segmentLine) {
        $segmentPath = if ($segmentLine.StartsWith("/api/v1/")) {
            $segmentLine.Substring("/api/v1".Length)
        } elseif ($segmentLine.StartsWith("http", [StringComparison]::OrdinalIgnoreCase)) {
            $segmentLine
        } else {
            "/media/videos/$($readyVideo.videoId)/hls/$segmentLine"
        }

        Invoke-Expected -Name "HLS segment" -Method "GET" -Path $segmentPath -Token $teacherToken -ExpectedStatus @(200, 206) | Out-Null
    } else {
        Add-Step -Name "HLS segment" -Method "GET" -Path "/media/videos/$($readyVideo.videoId)/hls/<segment>" -Status 0 -Expected "200/206" -Passed $false -Details "No segment line in playlist."
    }

    Invoke-Expected -Name "checkpoint list" -Method "GET" -Path "/checkpoints/video/$($readyVideo.videoId)" -Token $teacherToken -ExpectedStatus @(200) | Out-Null

    $slot = Get-FreeCheckpointSlot -Token $teacherToken -VideoId $readyVideo.videoId -DurationSeconds $readyVideo.durationSeconds
    $smokeCreate = Invoke-Expected -Name "create checkpoint" -Method "POST" -Path "/checkpoints" -Token $teacherToken -ExpectedStatus @(200) -Body @{
        videoId = $readyVideo.videoId
        timestampSeconds = $slot.timestamp
        orderNumber = $slot.order
        title = "$prefix Smoke create checkpoint"
        isRequired = $false
        isGraded = $false
    }

    if ($smokeCreate.status -eq 200) {
        Invoke-Expected -Name "cleanup smoke checkpoint" -Method "DELETE" -Path "/checkpoints/$($smokeCreate.data)" -Token $teacherToken -ExpectedStatus @(204) | Out-Null
    }

    $noQuestionPublish = Invoke-Expected -Name "graded checkpoint without question is blocked" -Method "POST" -Path "/checkpoints/$gradedDraft/publish" -Token $teacherToken -ExpectedStatus @(400)
    if ($noQuestionPublish.status -eq 400 -and ($noQuestionPublish.text -notmatch "GradedRequiresQuestions")) {
        ($steps[$steps.Count - 1]).passed = $false
        ($steps[$steps.Count - 1]).details = "Expected Checkpoint.GradedRequiresQuestions, got: $($noQuestionPublish.text)"
    }

    Invoke-Expected -Name "checkpoint with question publishes" -Method "POST" -Path "/checkpoints/$gradedPublished/publish" -Token $teacherToken -ExpectedStatus @(400) | Out-Null
    $lastStep = $steps[$steps.Count - 1]
    if ($lastStep.status -eq 400 -and $lastStep.details -match "AlreadyPublished") {
        $lastStep.passed = $true
        $lastStep.expected = "204 or already published 400"
    }

    Clear-EnrollmentByEmailAndCourse -Email $StudentEmail -CourseId $readyVideo.courseId
    Clear-EnrollmentByEmailAndCourse -Email $ForeignStudentEmail -CourseId $readyVideo.courseId

    $catalogBeforeEnroll = Invoke-Expected -Name "catalog detail before enrollment" -Method "GET" -Path "/catalog/courses/$($readyVideo.courseId)" -Token $studentToken -ExpectedStatus @(200)
    if ($catalogBeforeEnroll.status -eq 200 -and $catalogBeforeEnroll.data.isEnrolled -ne $false) {
        ($steps[$steps.Count - 1]).passed = $false
        ($steps[$steps.Count - 1]).details = "Expected isEnrolled=false before enrollment: $($catalogBeforeEnroll.text)"
    }

    $runtimeBeforeEnroll = Invoke-Expected -Name "runtime before enrollment blocked" -Method "GET" -Path "/student/lessons/$($readyVideo.lessonId)/runtime" -Token $studentToken -ExpectedStatus @(403)
    if ($runtimeBeforeEnroll.status -eq 403 -and ($runtimeBeforeEnroll.text -notmatch "Enrollment.Required")) {
        ($steps[$steps.Count - 1]).passed = $false
        ($steps[$steps.Count - 1]).details = "Expected Enrollment.Required, got: $($runtimeBeforeEnroll.text)"
    }

    Invoke-Expected -Name "student HLS before enrollment blocked" -Method "GET" -Path "/media/videos/$($readyVideo.videoId)/hls/master.m3u8" -Token $studentToken -ExpectedStatus @(403) | Out-Null
    Invoke-Expected -Name "student start attempt before enrollment blocked" -Method "POST" -Path "/lessons/$($readyVideo.lessonId)/attempts/start" -Token $studentToken -ExpectedStatus @(403) | Out-Null

    $enroll = Invoke-Expected -Name "student enrolls published course" -Method "POST" -Path "/catalog/courses/$($readyVideo.courseId)/enroll" -Token $studentToken -ExpectedStatus @(200)
    $repeatEnroll = Invoke-Expected -Name "student repeat enroll is idempotent" -Method "POST" -Path "/catalog/courses/$($readyVideo.courseId)/enroll" -Token $studentToken -ExpectedStatus @(200)
    if ($enroll.status -eq 200 -and $repeatEnroll.status -eq 200 -and ([string]$enroll.data.enrollmentId -ne [string]$repeatEnroll.data.enrollmentId)) {
        ($steps[$steps.Count - 1]).passed = $false
        ($steps[$steps.Count - 1]).details = "Expected same enrollment id, got $($enroll.data.enrollmentId) and $($repeatEnroll.data.enrollmentId)."
    }

    $myEnrollments = Invoke-Expected -Name "student my enrollments contains course" -Method "GET" -Path "/student/enrollments" -Token $studentToken -ExpectedStatus @(200)
    if ($myEnrollments.status -eq 200 -and @($myEnrollments.data | Where-Object { [string]$_.courseId -eq [string]$readyVideo.courseId }).Count -eq 0) {
        ($steps[$steps.Count - 1]).passed = $false
        ($steps[$steps.Count - 1]).details = "Expected course $($readyVideo.courseId) in my enrollments: $($myEnrollments.text)"
    }

    $dashboardBeforeAttempt = Invoke-Expected -Name "student dashboard after enrollment" -Method "GET" -Path "/student/dashboard" -Token $studentToken -ExpectedStatus @(200)
    if ($dashboardBeforeAttempt.status -eq 200) {
        $dashboardCourse = @($dashboardBeforeAttempt.data.enrolledCourses | Where-Object { [string]$_.courseId -eq [string]$readyVideo.courseId }) | Select-Object -First 1
        if (-not $dashboardCourse) {
            ($steps[$steps.Count - 1]).passed = $false
            ($steps[$steps.Count - 1]).details = "Expected course $($readyVideo.courseId) in dashboard: $($dashboardBeforeAttempt.text)"
        } elseif (
            $null -eq $dashboardCourse.publishedLessonsCount -or
            $null -eq $dashboardCourse.completedLessonsCount -or
            $null -eq $dashboardCourse.progressPercent -or
            $null -eq $dashboardCourse.lastActivityAtUtc
        ) {
            ($steps[$steps.Count - 1]).passed = $false
            ($steps[$steps.Count - 1]).details = "Dashboard course progress shape is incomplete: $($dashboardBeforeAttempt.text)"
        }

        if ($dashboardBeforeAttempt.text -match '"isCorrect"' -or $dashboardBeforeAttempt.text -match '"correctTextAnswer"') {
            ($steps[$steps.Count - 1]).passed = $false
            ($steps[$steps.Count - 1]).details = "Dashboard response leaked answer keys: $($dashboardBeforeAttempt.text)"
        }
    }

    $unpublishedCourseId = Get-UnpublishedCourseId -TeacherEmail $TeacherEmail
    if ($unpublishedCourseId) {
        Invoke-Expected -Name "student cannot enroll unpublished course" -Method "POST" -Path "/catalog/courses/$unpublishedCourseId/enroll" -Token $studentToken -ExpectedStatus @(400, 403, 404) | Out-Null
    } else {
        $limitations.Add("No unpublished course was available; enroll unpublished course smoke was skipped.") | Out-Null
    }

    $runtime = Invoke-Expected -Name "student lesson runtime after enrollment" -Method "GET" -Path "/student/lessons/$($readyVideo.lessonId)/runtime" -Token $studentToken -ExpectedStatus @(200)
    if ($runtime.status -eq 200 -and ($runtime.text -match '"isCorrect"' -or $runtime.text -match '"correctTextAnswer"')) {
        ($steps[$steps.Count - 1]).passed = $false
        ($steps[$steps.Count - 1]).details = "Runtime response leaked answer keys: $($runtime.text)"
    }
    if ($runtime.status -eq 200) {
        $runtimeCheckpointIds = @($runtime.data.checkpoints | ForEach-Object { [string]$_.checkpointId })
        if ($runtimeCheckpointIds -contains [string]$gradedDraft -or $runtimeCheckpointIds -contains [string]$archivedPractice) {
            ($steps[$steps.Count - 1]).passed = $false
            ($steps[$steps.Count - 1]).details = "Runtime response included draft/archived checkpoint. Draft=$gradedDraft Archived=$archivedPractice Body=$($runtime.text)"
        }
    }

    Invoke-Expected -Name "student runtime draft lesson blocked" -Method "GET" -Path "/student/lessons/$blockedRuntimeLesson/runtime" -Token $studentToken -ExpectedStatus @(404, 403) | Out-Null

    $catalogCourses = Invoke-Expected -Name "student catalog courses" -Method "GET" -Path "/catalog/courses?page=1&pageSize=20" -Token $studentToken -ExpectedStatus @(200)
    if ($catalogCourses.status -eq 200) {
        $catalogItems = @($catalogCourses.data.items)
        if ($null -eq $catalogCourses.data.page -or $null -eq $catalogCourses.data.pageSize -or $null -eq $catalogCourses.data.totalCount -or $null -eq $catalogCourses.data.totalPages) {
            ($steps[$steps.Count - 1]).passed = $false
            ($steps[$steps.Count - 1]).details = "Catalog response is not paginated: $($catalogCourses.text)"
        }

        try {
            Assert-CatalogCoursesPublished -Courses $catalogItems
        } catch {
            ($steps[$steps.Count - 1]).passed = $false
            ($steps[$steps.Count - 1]).details = $_.Exception.Message
        }
    }

    $catalogCourse = Invoke-Expected -Name "student opens catalog course" -Method "GET" -Path "/catalog/courses/$($readyVideo.courseId)" -Token $studentToken -ExpectedStatus @(200)
    if ($catalogCourse.status -eq 200) {
        if ($catalogCourse.data.isEnrolled -ne $true) {
            ($steps[$steps.Count - 1]).passed = $false
            ($steps[$steps.Count - 1]).details = "Expected isEnrolled=true after enrollment: $($catalogCourse.text)"
        }

        $catalogLessons = @($catalogCourse.data.lessons)
        try {
            Assert-CatalogLessonsPublished -Lessons $catalogLessons
        } catch {
            ($steps[$steps.Count - 1]).passed = $false
            ($steps[$steps.Count - 1]).details = $_.Exception.Message
        }

        $runtimeLessonFromCatalog = $catalogLessons |
            Where-Object { $_.videoReady -eq $true } |
            Select-Object -First 1

        if ($runtimeLessonFromCatalog) {
            Invoke-Expected -Name "catalog lesson opens runtime" -Method "GET" -Path "/student/lessons/$($runtimeLessonFromCatalog.id)/runtime" -Token $studentToken -ExpectedStatus @(200) | Out-Null
        } else {
            Add-Step -Name "catalog lesson opens runtime" -Method "GET" -Path "/student/lessons/<catalog-ready-lesson>/runtime" -Status 0 -Expected "200" -Passed $false -Details "Catalog course did not expose a videoReady lesson."
        }
    }

    $studentMaster = Invoke-Expected -Name "student HLS master published lesson" -Method "GET" -Path "/media/videos/$($readyVideo.videoId)/hls/master.m3u8" -Token $studentToken -ExpectedStatus @(200)
    $studentSegmentLine = ($studentMaster.text -split "`n" | ForEach-Object { $_.Trim() } | Where-Object { $_ -and -not $_.StartsWith("#") } | Select-Object -First 1)
    if ($studentSegmentLine) {
        $studentSegmentPath = if ($studentSegmentLine.StartsWith("/api/v1/")) {
            $studentSegmentLine.Substring("/api/v1".Length)
        } elseif ($studentSegmentLine.StartsWith("http", [StringComparison]::OrdinalIgnoreCase)) {
            $studentSegmentLine
        } else {
            "/media/videos/$($readyVideo.videoId)/hls/$studentSegmentLine"
        }

        Invoke-Expected -Name "student HLS segment published lesson" -Method "GET" -Path $studentSegmentPath -Token $studentToken -ExpectedStatus @(200, 206) | Out-Null
    } else {
        Add-Step -Name "student HLS segment published lesson" -Method "GET" -Path "/media/videos/$($readyVideo.videoId)/hls/<segment>" -Status 0 -Expected "200/206" -Passed $false -Details "No segment line in student playlist."
    }

    $startAttempt = Invoke-Expected -Name "student start attempt" -Method "POST" -Path "/lessons/$($readyVideo.lessonId)/attempts/start" -Token $studentToken -ExpectedStatus @(200)
    $repeatStartAttempt = Invoke-Expected -Name "student repeat start returns active attempt" -Method "POST" -Path "/lessons/$($readyVideo.lessonId)/attempts/start" -Token $studentToken -ExpectedStatus @(200)
    if ($startAttempt.status -eq 200 -and $repeatStartAttempt.status -eq 200 -and ([string]$startAttempt.data.attemptId -ne [string]$repeatStartAttempt.data.attemptId)) {
        ($steps[$steps.Count - 1]).passed = $false
        ($steps[$steps.Count - 1]).details = "Expected same active attempt id, got $($startAttempt.data.attemptId) and $($repeatStartAttempt.data.attemptId)."
    }

    $activeAttempt = Invoke-Expected -Name "student active attempt" -Method "GET" -Path "/lessons/$($readyVideo.lessonId)/attempts/active" -Token $studentToken -ExpectedStatus @(200)

    $correctOptionId = Get-CorrectOptionIdBySql -QuestionId $questionId
    if ($startAttempt.status -eq 200 -and $correctOptionId) {
        Invoke-Expected -Name "student submit correct SingleChoice answer" -Method "POST" -Path "/attempts/$($startAttempt.data.attemptId)/answers" -Token $studentToken -ExpectedStatus @(200) -Body @{
            questionId = $questionId
            selectedOptionIds = @($correctOptionId)
            textAnswer = $null
        } | Out-Null

        $duplicateSubmit = Invoke-Expected -Name "student retry answer replaces previous answer" -Method "POST" -Path "/attempts/$($startAttempt.data.attemptId)/answers" -Token $studentToken -ExpectedStatus @(200) -Body @{
            questionId = $questionId
            selectedOptionIds = @($correctOptionId)
            textAnswer = $null
        }
        if ($duplicateSubmit.status -eq 200) {
            $answerCount = Get-AttemptAnswerCountBySql -AttemptId $startAttempt.data.attemptId -QuestionId $questionId
            if ($answerCount -ne 1) {
                ($steps[$steps.Count - 1]).passed = $false
                ($steps[$steps.Count - 1]).details = "Expected retry to replace the previous answer, but found $answerCount rows for attempt/question."
            }
        }

        $activeAfterAnswer = Invoke-Expected -Name "active attempt includes answered question" -Method "GET" -Path "/lessons/$($readyVideo.lessonId)/attempts/active" -Token $studentToken -ExpectedStatus @(200)
        if ($activeAfterAnswer.status -eq 200 -and @($activeAfterAnswer.data.answeredQuestionIds | Where-Object { [string]$_ -eq [string]$questionId }).Count -eq 0) {
            ($steps[$steps.Count - 1]).passed = $false
            ($steps[$steps.Count - 1]).details = "answeredQuestionIds does not contain $questionId."
        }

        Invoke-Expected -Name "foreign student enrolls course" -Method "POST" -Path "/catalog/courses/$($readyVideo.courseId)/enroll" -Token $foreignStudentToken -ExpectedStatus @(200) | Out-Null
        $foreignStart = Invoke-Expected -Name "foreign student start attempt" -Method "POST" -Path "/lessons/$($readyVideo.lessonId)/attempts/start" -Token $foreignStudentToken -ExpectedStatus @(200)
        if ($foreignStart.status -eq 200) {
            Invoke-Expected -Name "student cannot submit to foreign attempt" -Method "POST" -Path "/attempts/$($foreignStart.data.attemptId)/answers" -Token $studentToken -ExpectedStatus @(403) -Body @{
                questionId = $questionId
                selectedOptionIds = @($correctOptionId)
                textAnswer = $null
            } | Out-Null

            Invoke-Expected -Name "student cannot read foreign result" -Method "GET" -Path "/attempts/$($foreignStart.data.attemptId)/result" -Token $studentToken -ExpectedStatus @(403) | Out-Null
        }

        $otherQuestionId = Get-QuestionFromAnotherLessonBySql -QuestionId $questionId -LessonId $readyVideo.lessonId
        if ($otherQuestionId) {
            Invoke-Expected -Name "student cannot submit question from another lesson" -Method "POST" -Path "/attempts/$($startAttempt.data.attemptId)/answers" -Token $studentToken -ExpectedStatus @(400) -Body @{
                questionId = $otherQuestionId
                selectedOptionIds = @()
                textAnswer = $null
            } | Out-Null
        } else {
            $limitations.Add("No question from another lesson was available; Attempt.QuestionMismatch smoke was skipped.") | Out-Null
        }

        Invoke-Expected -Name "student result before completion blocked" -Method "GET" -Path "/attempts/$($startAttempt.data.attemptId)/result" -Token $studentToken -ExpectedStatus @(400) | Out-Null

        Invoke-Expected -Name "student complete attempt" -Method "POST" -Path "/attempts/$($startAttempt.data.attemptId)/complete" -Token $studentToken -ExpectedStatus @(200) | Out-Null
        $ownResult = Invoke-Expected -Name "student reads own result review" -Method "GET" -Path "/attempts/$($startAttempt.data.attemptId)/result" -Token $studentToken -ExpectedStatus @(200)
        if ($ownResult.status -eq 200) {
            $resultAnswers = @($ownResult.data.answers)
            $firstAnswer = $resultAnswers | Select-Object -First 1

            if (
                $resultAnswers.Count -eq 0 -or
                $null -eq $ownResult.data.totalQuestions -or
                $null -eq $ownResult.data.correctAnswers -or
                $null -eq $firstAnswer.selectedOptions -or
                $null -eq $firstAnswer.isCorrect -or
                $null -eq $firstAnswer.correctOptions
            ) {
                ($steps[$steps.Count - 1]).passed = $false
                ($steps[$steps.Count - 1]).details = "Attempt result review shape is incomplete: $($ownResult.text)"
            }

            $optionHasIsCorrect = $false
            foreach ($answer in $resultAnswers) {
                foreach ($option in @($answer.selectedOptions) + @($answer.correctOptions)) {
                    if ($option -and $option.PSObject.Properties.Name -contains "isCorrect") {
                        $optionHasIsCorrect = $true
                    }
                }
            }

            if ($optionHasIsCorrect) {
                ($steps[$steps.Count - 1]).passed = $false
                ($steps[$steps.Count - 1]).details = "Attempt result option DTO leaked isCorrect: $($ownResult.text)"
            }

            if ($firstAnswer.revealCorrectAnswer -eq $true -and @($firstAnswer.correctOptions).Count -eq 0) {
                ($steps[$steps.Count - 1]).passed = $false
                ($steps[$steps.Count - 1]).details = "Expected correctOptions when revealCorrectAnswer is true: $($ownResult.text)"
            }
        }

        $dashboardAfterComplete = Invoke-Expected -Name "student dashboard after completed attempt" -Method "GET" -Path "/student/dashboard" -Token $studentToken -ExpectedStatus @(200)
        if ($dashboardAfterComplete.status -eq 200) {
            $dashboardCourse = @($dashboardAfterComplete.data.enrolledCourses | Where-Object { [string]$_.courseId -eq [string]$readyVideo.courseId }) | Select-Object -First 1
            $recentAttempt = @($dashboardAfterComplete.data.recentAttempts | Where-Object { [string]$_.attemptId -eq [string]$startAttempt.data.attemptId }) | Select-Object -First 1

            if (-not $dashboardCourse) {
                ($steps[$steps.Count - 1]).passed = $false
                ($steps[$steps.Count - 1]).details = "Expected course $($readyVideo.courseId) in dashboard after completion: $($dashboardAfterComplete.text)"
            } elseif ([int]$dashboardCourse.completedLessonsCount -lt 1) {
                ($steps[$steps.Count - 1]).passed = $false
                ($steps[$steps.Count - 1]).details = "Expected completedLessonsCount >= 1: $($dashboardAfterComplete.text)"
            } elseif ([int]$dashboardCourse.publishedLessonsCount -gt 0 -and [int]$dashboardCourse.progressPercent -le 0) {
                ($steps[$steps.Count - 1]).passed = $false
                ($steps[$steps.Count - 1]).details = "Expected positive progressPercent: $($dashboardAfterComplete.text)"
            } elseif (-not $recentAttempt) {
                ($steps[$steps.Count - 1]).passed = $false
                ($steps[$steps.Count - 1]).details = "Expected completed attempt in recentAttempts: $($dashboardAfterComplete.text)"
            }

            if ($dashboardAfterComplete.text -match '"isCorrect"' -or $dashboardAfterComplete.text -match '"correctTextAnswer"') {
                ($steps[$steps.Count - 1]).passed = $false
                ($steps[$steps.Count - 1]).details = "Dashboard response leaked answer keys: $($dashboardAfterComplete.text)"
            }
        }
    } else {
        $limitations.Add("Student attempt answer smoke skipped because attempt or correct answer option was unavailable.") | Out-Null
    }

    Invoke-Expected -Name "lesson archive" -Method "POST" -Path "/lessons/$lesson1/archive" -Token $teacherToken -ExpectedStatus @(204) | Out-Null
    Invoke-Expected -Name "lesson publish again" -Method "POST" -Path "/lessons/$lesson1/publish" -Token $teacherToken -ExpectedStatus @(204) | Out-Null

    Invoke-Expected -Name "checkpoint archive" -Method "POST" -Path "/checkpoints/$practicePublished/archive" -Token $teacherToken -ExpectedStatus @(204) | Out-Null
    Invoke-Expected -Name "checkpoint publish again" -Method "POST" -Path "/checkpoints/$practicePublished/publish" -Token $teacherToken -ExpectedStatus @(204) | Out-Null

    Invoke-Expected -Name "student cannot create teacher course" -Method "POST" -Path "/courses" -Token $studentToken -ExpectedStatus @(403) -Body @{
        title = "$prefix Student forbidden course"
        description = "Must not be created."
    } | Out-Null

    Invoke-Expected -Name "student cannot create checkpoint" -Method "POST" -Path "/checkpoints" -Token $studentToken -ExpectedStatus @(403) -Body @{
        videoId = $readyVideo.videoId
        timestampSeconds = 1
        orderNumber = 999
        title = "$prefix Student forbidden checkpoint"
        isRequired = $false
        isGraded = $false
    } | Out-Null

}

$logCheck = docker logs --since $smokeStartedAt MedHub.Api 2>&1 | Select-String -Pattern "\b500\b|Exception|Npgsql" -CaseSensitive:$false
$no500 = @($logCheck).Count -eq 0
Add-Step -Name "no 500 in backend logs" -Method "docker logs" -Path "MedHub.Api --since $smokeStartedAt" -Status $(if ($no500) { 0 } else { 1 }) -Expected "no 500/Exception/Npgsql" -Passed $no500 -Details (($logCheck | Out-String).Trim())

$createdItems = @()
foreach ($item in $created) {
    $createdItems += [string]$item
}

$knownLimitations = @()
foreach ($item in $limitations) {
    $knownLimitations += [string]$item
}
$knownLimitations = @($knownLimitations | Sort-Object -Unique)

$stepItems = @()
foreach ($item in $steps) {
    $stepItems += $item
}

$report = [pscustomobject]@{
    apiBase = $ApiBase
    createdOrReused = $createdItems
    knownLimitations = $knownLimitations
    steps = $stepItems
    summary = [pscustomobject]@{
        total = $stepItems.Count
        passed = @($stepItems | Where-Object { $_.passed }).Count
        failed = @($stepItems | Where-Object { -not $_.passed }).Count
    }
}

$reportJson = $report | ConvertTo-Json -Depth 20
$reportPath = Join-Path (Get-Location) "scripts/dev-seed-smoke-report.json"
$reportJson | Set-Content -Encoding utf8 -Path $reportPath

Write-Host ""
Write-Host "Summary: $($report.summary.passed)/$($report.summary.total) passed"
Write-Host "Report: $reportPath"
Write-Host ""
$steps | Format-Table name, method, path, status, expected, passed -AutoSize

if ($report.summary.failed -gt 0) {
    exit 1
}
