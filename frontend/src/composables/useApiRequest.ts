import {
  apiRequest,
  type ApiRequestOptions,
  type ApiResponse,
} from '@/helpers/api';
import { useToast } from 'vue-toastification';

export const useApiRequest = () => {
  const toast = useToast();

  return async <T>(url: string, options: ApiRequestOptions): Promise<ApiResponse<T>> => {
    return await apiRequest<T>(url, options, toast);
  };
};
