import type { ChunkUploadUrl, PartETag } from '../api/teacherMediaApi';

export type UploadChunksInput = {
  file: File;
  chunkSize: number;
  chunkUploadUrls: ChunkUploadUrl[];
  onProgress?: (progress: {
    uploadedBytes: number;
    totalBytes: number;
    uploadedParts: number;
    totalParts: number;
  }) => void;
  signal?: AbortSignal;
};

export async function uploadChunks({
  file,
  chunkSize,
  chunkUploadUrls,
  onProgress,
  signal,
}: UploadChunksInput): Promise<PartETag[]> {
  const sortedUrls = [...chunkUploadUrls].sort((left, right) => left.partNumber - right.partNumber);
  const partETags: PartETag[] = [];
  let uploadedBytes = 0;

  for (const chunkUploadUrl of sortedUrls) {
    signal?.throwIfAborted();

    const start = (chunkUploadUrl.partNumber - 1) * chunkSize;
    const end = Math.min(start + chunkSize, file.size);
    const chunk = file.slice(start, end);

    const response = await fetch(chunkUploadUrl.uploadUrl, {
      method: 'PUT',
      body: chunk,
      signal,
    });

    if (!response.ok) {
      throw new Error(`Не удалось загрузить часть ${chunkUploadUrl.partNumber}: ${response.status}`);
    }

    const eTag = response.headers.get('ETag') ?? response.headers.get('etag');

    if (!eTag) {
      throw new Error('Upload succeeded, but ETag header is not exposed. Check MinIO CORS exposeHeaders.');
    }

    uploadedBytes += chunk.size;
    partETags.push({
      partNumber: chunkUploadUrl.partNumber,
      eTag,
    });

    onProgress?.({
      uploadedBytes,
      totalBytes: file.size,
      uploadedParts: partETags.length,
      totalParts: sortedUrls.length,
    });
  }

  return partETags.sort((left, right) => left.partNumber - right.partNumber);
}
