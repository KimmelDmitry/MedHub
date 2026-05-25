/* eslint-disable */
/* tslint:disable */
// @ts-nocheck
/*
 * ---------------------------------------------------------------
 * ## THIS FILE WAS GENERATED VIA SWAGGER-TYPESCRIPT-API        ##
 * ##                                                           ##
 * ## AUTHOR: acacode                                           ##
 * ## SOURCE: https://github.com/acacode/swagger-typescript-api ##
 * ---------------------------------------------------------------
 */

/** @format int32 */
export enum QuestionType {
  Value1 = 1,
  Value2 = 2,
  Value3 = 3,
  Value4 = 4,
}

/** @format int32 */
export enum LessonContentType {
  Value1 = 1,
  Value2 = 2,
  Value3 = 3,
}

export interface AddAnswerOptionRequest {
  text?: string | null;
  isCorrect?: boolean;
}

export interface AttachVideoToLessonRequest {
  /** @format uuid */
  videoId?: string;
}

export interface CompleteVideoUploadRequest {
  uploadId?: string | null;
  partETags?: PartETagDto[] | null;
}

export interface CreateCheckpointRequest {
  /** @format uuid */
  videoId?: string;
  /** @format int32 */
  timestampSeconds?: number;
  /** @format int32 */
  orderNumber?: number;
  title?: string | null;
  isRequired?: boolean;
  isGraded?: boolean;
}

export interface CreateCourseRequest {
  title?: string | null;
  description?: string | null;
}

export interface CreateLessonRequest {
  /** @format uuid */
  courseId?: string;
  title?: string | null;
  /** @format int32 */
  order?: number;
  contentType?: LessonContentType;
  contentUrl?: string | null;
}

export interface CreateQuestionRequest {
  text?: string | null;
  type?: QuestionType;
  allowRetry?: boolean;
  /** @format int32 */
  timeLimitSeconds?: number | null;
  revealCorrectAnswer?: boolean;
  correctTextAnswer?: string | null;
}

export interface LogInUserRequest {
  email?: string | null;
  password?: string | null;
}

export interface PartETagDto {
  /** @format int32 */
  partNumber?: number;
  eTag?: string | null;
}

export interface RegisterUserRequest {
  email?: string | null;
  firstName?: string | null;
  lastName?: string | null;
  password?: string | null;
}

export interface StartVideoUploadRequest {
  /** @format uuid */
  lessonId?: string;
  fileName?: string | null;
  contentType?: string | null;
  /** @format int64 */
  sizeBytes?: number;
}

export interface SubmitCheckpointAnswerRequest {
  /** @format uuid */
  questionId?: string;
  selectedOptionIds?: string[] | null;
  textAnswer?: string | null;
}

export interface UpdateAnswerOptionRequest {
  text?: string | null;
  isCorrect?: boolean;
}

export interface UpdateCheckpointRequest {
  title?: string | null;
  /** @format int32 */
  timestampSeconds?: number | null;
  /** @format int32 */
  orderNumber?: number | null;
  isRequired?: boolean | null;
  isGraded?: boolean | null;
}

export interface UpdateCourseDescriptionRequest {
  description?: string | null;
}

export interface UpdateCourseTitleRequest {
  title?: string | null;
}

export interface UpdateLessonContentRequest {
  contentUrl?: string | null;
  contentType?: LessonContentType;
}

export interface UpdateLessonOrderRequest {
  /** @format int32 */
  order?: number;
}

export interface UpdateLessonTitleRequest {
  title?: string | null;
}

export interface UpdateQuestionSettingsRequest {
  allowRetry?: boolean;
  /** @format int32 */
  timeLimitSeconds?: number | null;
  revealCorrectAnswer?: boolean;
  correctTextAnswer?: string | null;
}

export interface UpdateQuestionTextRequest {
  text?: string | null;
}

import type {
  AxiosInstance,
  AxiosRequestConfig,
  AxiosResponse,
  HeadersDefaults,
  ResponseType,
} from "axios";
import axios from "axios";

export type QueryParamsType = Record<string | number, any>;

export interface FullRequestParams
  extends Omit<AxiosRequestConfig, "data" | "params" | "url" | "responseType"> {
  /** set parameter to `true` for call `securityWorker` for this request */
  secure?: boolean;
  /** request path */
  path: string;
  /** content type of request body */
  type?: ContentType;
  /** query params */
  query?: QueryParamsType;
  /** format of response (i.e. response.json() -> format: "json") */
  format?: ResponseType;
  /** request body */
  body?: unknown;
}

export type RequestParams = Omit<
  FullRequestParams,
  "body" | "method" | "query" | "path"
>;

export interface ApiConfig<SecurityDataType = unknown>
  extends Omit<AxiosRequestConfig, "data" | "cancelToken"> {
  securityWorker?: (
    securityData: SecurityDataType | null,
  ) => Promise<AxiosRequestConfig | void> | AxiosRequestConfig | void;
  secure?: boolean;
  format?: ResponseType;
}

export enum ContentType {
  Json = "application/json",
  JsonApi = "application/vnd.api+json",
  FormData = "multipart/form-data",
  UrlEncoded = "application/x-www-form-urlencoded",
  Text = "text/plain",
}

export class HttpClient<SecurityDataType = unknown> {
  public instance: AxiosInstance;
  private securityData: SecurityDataType | null = null;
  private securityWorker?: ApiConfig<SecurityDataType>["securityWorker"];
  private secure?: boolean;
  private format?: ResponseType;

  constructor({
    securityWorker,
    secure,
    format,
    ...axiosConfig
  }: ApiConfig<SecurityDataType> = {}) {
    this.instance = axios.create({
      ...axiosConfig,
      baseURL: axiosConfig.baseURL || "",
    });
    this.secure = secure;
    this.format = format;
    this.securityWorker = securityWorker;
  }

  public setSecurityData = (data: SecurityDataType | null) => {
    this.securityData = data;
  };

  protected mergeRequestParams(
    params1: AxiosRequestConfig,
    params2?: AxiosRequestConfig,
  ): AxiosRequestConfig {
    const method = params1.method || (params2 && params2.method);

    return {
      ...this.instance.defaults,
      ...params1,
      ...(params2 || {}),
      headers: {
        ...((method &&
          this.instance.defaults.headers[
            method.toLowerCase() as keyof HeadersDefaults
          ]) ||
          {}),
        ...(params1.headers || {}),
        ...((params2 && params2.headers) || {}),
      },
    };
  }

  protected stringifyFormItem(formItem: unknown) {
    if (typeof formItem === "object" && formItem !== null) {
      return JSON.stringify(formItem);
    } else {
      return `${formItem}`;
    }
  }

  protected createFormData(input: Record<string, unknown>): FormData {
    if (input instanceof FormData) {
      return input;
    }
    return Object.keys(input || {}).reduce((formData, key) => {
      const property = input[key];
      const propertyContent: any[] =
        property instanceof Array ? property : [property];

      for (const formItem of propertyContent) {
        const isFileType = formItem instanceof Blob || formItem instanceof File;
        formData.append(
          key,
          isFileType ? formItem : this.stringifyFormItem(formItem),
        );
      }

      return formData;
    }, new FormData());
  }

  public request = async <T = any, _E = any>({
    secure,
    path,
    type,
    query,
    format,
    body,
    ...params
  }: FullRequestParams): Promise<AxiosResponse<T>> => {
    const secureParams =
      ((typeof secure === "boolean" ? secure : this.secure) &&
        this.securityWorker &&
        (await this.securityWorker(this.securityData))) ||
      {};
    const requestParams = this.mergeRequestParams(params, secureParams);
    const responseFormat = format || this.format || undefined;

    if (
      type === ContentType.FormData &&
      body &&
      body !== null &&
      typeof body === "object"
    ) {
      body = this.createFormData(body as Record<string, unknown>);
    }

    if (
      type === ContentType.Text &&
      body &&
      body !== null &&
      typeof body !== "string"
    ) {
      body = JSON.stringify(body);
    }

    return this.instance.request({
      ...requestParams,
      headers: {
        ...(requestParams.headers || {}),
        ...(type ? { "Content-Type": type } : {}),
      },
      params: query,
      responseType: responseFormat,
      data: body,
      url: path,
    });
  };
}

/**
 * @title MedHub.Api v1
 * @version 1
 */
export class Api<
  SecurityDataType extends unknown,
> extends HttpClient<SecurityDataType> {
  api = {
    /**
     * No description
     *
     * @tags Attempts
     * @name V1LessonsAttemptsStartCreate
     * @request POST:/api/v1/lessons/{lessonId}/attempts/start
     */
    v1LessonsAttemptsStartCreate: (
      lessonId: string,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/v1/lessons/${lessonId}/attempts/start`,
        method: "POST",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Attempts
     * @name V1LessonsAttemptsActiveList
     * @request GET:/api/v1/lessons/{lessonId}/attempts/active
     */
    v1LessonsAttemptsActiveList: (
      lessonId: string,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/v1/lessons/${lessonId}/attempts/active`,
        method: "GET",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Attempts
     * @name V1AttemptsAnswersCreate
     * @request POST:/api/v1/attempts/{attemptId}/answers
     */
    v1AttemptsAnswersCreate: (
      attemptId: string,
      data: SubmitCheckpointAnswerRequest,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/v1/attempts/${attemptId}/answers`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Attempts
     * @name V1AttemptsCompleteCreate
     * @request POST:/api/v1/attempts/{attemptId}/complete
     */
    v1AttemptsCompleteCreate: (attemptId: string, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/v1/attempts/${attemptId}/complete`,
        method: "POST",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Attempts
     * @name V1AttemptsResultList
     * @request GET:/api/v1/attempts/{attemptId}/result
     */
    v1AttemptsResultList: (attemptId: string, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/v1/attempts/${attemptId}/result`,
        method: "GET",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Checkpoints
     * @name V1CheckpointsDetail
     * @request GET:/api/v1/checkpoints/{checkpointId}
     */
    v1CheckpointsDetail: (checkpointId: string, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/v1/checkpoints/${checkpointId}`,
        method: "GET",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Checkpoints
     * @name V1CheckpointsPartialUpdate
     * @request PATCH:/api/v1/checkpoints/{checkpointId}
     */
    v1CheckpointsPartialUpdate: (
      checkpointId: string,
      data: UpdateCheckpointRequest,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/v1/checkpoints/${checkpointId}`,
        method: "PATCH",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Checkpoints
     * @name V1CheckpointsDelete
     * @request DELETE:/api/v1/checkpoints/{checkpointId}
     */
    v1CheckpointsDelete: (checkpointId: string, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/v1/checkpoints/${checkpointId}`,
        method: "DELETE",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Checkpoints
     * @name V1CheckpointsVideoDetail
     * @request GET:/api/v1/checkpoints/video/{videoId}
     */
    v1CheckpointsVideoDetail: (videoId: string, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/v1/checkpoints/video/${videoId}`,
        method: "GET",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Checkpoints
     * @name V1CheckpointsCreate
     * @request POST:/api/v1/checkpoints
     */
    v1CheckpointsCreate: (
      data: CreateCheckpointRequest,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/v1/checkpoints`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Checkpoints
     * @name V1CheckpointsPublishCreate
     * @request POST:/api/v1/checkpoints/{checkpointId}/publish
     */
    v1CheckpointsPublishCreate: (
      checkpointId: string,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/v1/checkpoints/${checkpointId}/publish`,
        method: "POST",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Checkpoints
     * @name V1CheckpointsArchiveCreate
     * @request POST:/api/v1/checkpoints/{checkpointId}/archive
     */
    v1CheckpointsArchiveCreate: (
      checkpointId: string,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/v1/checkpoints/${checkpointId}/archive`,
        method: "POST",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Courses
     * @name V1CoursesDetail
     * @request GET:/api/v1/courses/{courseId}
     */
    v1CoursesDetail: (courseId: string, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/v1/courses/${courseId}`,
        method: "GET",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Courses
     * @name V1CoursesContentList
     * @request GET:/api/v1/courses/{courseId}/content
     */
    v1CoursesContentList: (courseId: string, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/v1/courses/${courseId}/content`,
        method: "GET",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Courses
     * @name V1CoursesCreate
     * @request POST:/api/v1/courses
     */
    v1CoursesCreate: (data: CreateCourseRequest, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/v1/courses`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Courses
     * @name V1CoursesPublishCreate
     * @request POST:/api/v1/courses/{courseId}/publish
     */
    v1CoursesPublishCreate: (courseId: string, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/v1/courses/${courseId}/publish`,
        method: "POST",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Courses
     * @name V1CoursesArchiveCreate
     * @request POST:/api/v1/courses/{courseId}/archive
     */
    v1CoursesArchiveCreate: (courseId: string, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/v1/courses/${courseId}/archive`,
        method: "POST",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Courses
     * @name V1CoursesTitlePartialUpdate
     * @request PATCH:/api/v1/courses/{courseId}/title
     */
    v1CoursesTitlePartialUpdate: (
      courseId: string,
      data: UpdateCourseTitleRequest,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/v1/courses/${courseId}/title`,
        method: "PATCH",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Courses
     * @name V1CoursesDescriptionPartialUpdate
     * @request PATCH:/api/v1/courses/{courseId}/description
     */
    v1CoursesDescriptionPartialUpdate: (
      courseId: string,
      data: UpdateCourseDescriptionRequest,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/v1/courses/${courseId}/description`,
        method: "PATCH",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Lessons
     * @name V1LessonsDetail
     * @request GET:/api/v1/lessons/{lessonId}
     */
    v1LessonsDetail: (lessonId: string, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/v1/lessons/${lessonId}`,
        method: "GET",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Lessons
     * @name V1LessonsCreate
     * @request POST:/api/v1/lessons
     */
    v1LessonsCreate: (data: CreateLessonRequest, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/v1/lessons`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Lessons
     * @name V1LessonsPublishCreate
     * @request POST:/api/v1/lessons/{lessonId}/publish
     */
    v1LessonsPublishCreate: (lessonId: string, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/v1/lessons/${lessonId}/publish`,
        method: "POST",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Lessons
     * @name V1LessonsArchiveCreate
     * @request POST:/api/v1/lessons/{lessonId}/archive
     */
    v1LessonsArchiveCreate: (lessonId: string, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/v1/lessons/${lessonId}/archive`,
        method: "POST",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Lessons
     * @name V1LessonsTitlePartialUpdate
     * @request PATCH:/api/v1/lessons/{lessonId}/title
     */
    v1LessonsTitlePartialUpdate: (
      lessonId: string,
      data: UpdateLessonTitleRequest,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/v1/lessons/${lessonId}/title`,
        method: "PATCH",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Lessons
     * @name V1LessonsOrderPartialUpdate
     * @request PATCH:/api/v1/lessons/{lessonId}/order
     */
    v1LessonsOrderPartialUpdate: (
      lessonId: string,
      data: UpdateLessonOrderRequest,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/v1/lessons/${lessonId}/order`,
        method: "PATCH",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Lessons
     * @name V1LessonsContentPartialUpdate
     * @request PATCH:/api/v1/lessons/{lessonId}/content
     */
    v1LessonsContentPartialUpdate: (
      lessonId: string,
      data: UpdateLessonContentRequest,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/v1/lessons/${lessonId}/content`,
        method: "PATCH",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Lessons
     * @name V1LessonsAttachVideoCreate
     * @request POST:/api/v1/lessons/{lessonId}/attach-video
     */
    v1LessonsAttachVideoCreate: (
      lessonId: string,
      data: AttachVideoToLessonRequest,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/v1/lessons/${lessonId}/attach-video`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Media
     * @name V1MediaVideosStartUploadCreate
     * @request POST:/api/v1/media/videos/start-upload
     */
    v1MediaVideosStartUploadCreate: (
      data: StartVideoUploadRequest,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/v1/media/videos/start-upload`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Media
     * @name V1MediaVideosCompleteUploadCreate
     * @request POST:/api/v1/media/videos/{videoId}/complete-upload
     */
    v1MediaVideosCompleteUploadCreate: (
      videoId: string,
      data: CompleteVideoUploadRequest,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/v1/media/videos/${videoId}/complete-upload`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Media
     * @name V1MediaVideosAbortUploadCreate
     * @request POST:/api/v1/media/videos/{videoId}/abort-upload
     */
    v1MediaVideosAbortUploadCreate: (
      videoId: string,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/v1/media/videos/${videoId}/abort-upload`,
        method: "POST",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Media
     * @name V1MediaVideosStatusList
     * @request GET:/api/v1/media/videos/{videoId}/status
     */
    v1MediaVideosStatusList: (videoId: string, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/v1/media/videos/${videoId}/status`,
        method: "GET",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Media
     * @name V1MediaVideosPlaybackList
     * @request GET:/api/v1/media/videos/{videoId}/playback
     */
    v1MediaVideosPlaybackList: (videoId: string, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/v1/media/videos/${videoId}/playback`,
        method: "GET",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Questions
     * @name V1QuestionsDetail
     * @request GET:/api/v1/questions/{questionId}
     */
    v1QuestionsDetail: (questionId: string, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/v1/questions/${questionId}`,
        method: "GET",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Questions
     * @name V1QuestionsDelete
     * @request DELETE:/api/v1/questions/{questionId}
     */
    v1QuestionsDelete: (questionId: string, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/v1/questions/${questionId}`,
        method: "DELETE",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Questions
     * @name V1CheckpointsQuestionsList
     * @request GET:/api/v1/checkpoints/{checkpointId}/questions
     */
    v1CheckpointsQuestionsList: (
      checkpointId: string,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/v1/checkpoints/${checkpointId}/questions`,
        method: "GET",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Questions
     * @name V1CheckpointsQuestionsCreate
     * @request POST:/api/v1/checkpoints/{checkpointId}/questions
     */
    v1CheckpointsQuestionsCreate: (
      checkpointId: string,
      data: CreateQuestionRequest,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/v1/checkpoints/${checkpointId}/questions`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Questions
     * @name V1QuestionsTextPartialUpdate
     * @request PATCH:/api/v1/questions/{questionId}/text
     */
    v1QuestionsTextPartialUpdate: (
      questionId: string,
      data: UpdateQuestionTextRequest,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/v1/questions/${questionId}/text`,
        method: "PATCH",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Questions
     * @name V1QuestionsSettingsPartialUpdate
     * @request PATCH:/api/v1/questions/{questionId}/settings
     */
    v1QuestionsSettingsPartialUpdate: (
      questionId: string,
      data: UpdateQuestionSettingsRequest,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/v1/questions/${questionId}/settings`,
        method: "PATCH",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Questions
     * @name V1QuestionsOptionsCreate
     * @request POST:/api/v1/questions/{questionId}/options
     */
    v1QuestionsOptionsCreate: (
      questionId: string,
      data: AddAnswerOptionRequest,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/v1/questions/${questionId}/options`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Questions
     * @name V1QuestionsOptionsPartialUpdate
     * @request PATCH:/api/v1/questions/{questionId}/options/{answerOptionId}
     */
    v1QuestionsOptionsPartialUpdate: (
      questionId: string,
      answerOptionId: string,
      data: UpdateAnswerOptionRequest,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/v1/questions/${questionId}/options/${answerOptionId}`,
        method: "PATCH",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Questions
     * @name V1QuestionsOptionsDelete
     * @request DELETE:/api/v1/questions/{questionId}/options/{answerOptionId}
     */
    v1QuestionsOptionsDelete: (
      questionId: string,
      answerOptionId: string,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/v1/questions/${questionId}/options/${answerOptionId}`,
        method: "DELETE",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Users
     * @name V1UsersMeList
     * @request GET:/api/v1/users/me
     */
    v1UsersMeList: (params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/v1/users/me`,
        method: "GET",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Users
     * @name V1UsersRegisterCreate
     * @request POST:/api/v1/users/register
     */
    v1UsersRegisterCreate: (
      data: RegisterUserRequest,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/v1/users/register`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Users
     * @name V1UsersLoginCreate
     * @request POST:/api/v1/users/login
     */
    v1UsersLoginCreate: (data: LogInUserRequest, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/v1/users/login`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        ...params,
      }),
  };
}
