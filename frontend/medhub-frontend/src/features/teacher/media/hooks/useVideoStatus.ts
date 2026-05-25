import { useQuery } from '@tanstack/react-query';
import { getVideoPlayback, getVideoStatus, teacherVideoQueryKey } from '../api/teacherMediaApi';

function isNonTerminalStatus(status?: string | null) {
  const normalizedStatus = status?.trim().toLowerCase();
  return normalizedStatus === 'uploading' || normalizedStatus === 'uploaded' || normalizedStatus === 'processing';
}

export function useVideoStatus(videoId: string | undefined | null) {
  return useQuery({
    queryKey: [...teacherVideoQueryKey, videoId, 'status'],
    queryFn: () => getVideoStatus(videoId!),
    enabled: Boolean(videoId),
    refetchInterval: (query) => (isNonTerminalStatus(query.state.data?.status) ? 3000 : false),
  });
}

export function useVideoPlayback(videoId: string | undefined | null, enabled: boolean) {
  return useQuery({
    queryKey: [...teacherVideoQueryKey, videoId, 'playback'],
    queryFn: () => getVideoPlayback(videoId!),
    enabled: Boolean(videoId) && enabled,
    retry: false,
  });
}
