import {
  apiRequest,
  type ApiRequestOptions,
  type ApiResponse,
} from '@/helpers/api';
import { ref } from 'vue';
import { useToast } from 'vue-toastification';

export const useApiRequest = () => {
  const isLoading = ref(false);
  const toast = useToast();

  const request = async <T>(
    url: string,
    options: ApiRequestOptions
  ): Promise<ApiResponse<T>> => {
    isLoading.value = true;

    try {
      return await apiRequest<T>(url, options, toast);
    } finally {
      isLoading.value = false;
    }
  };

  return { request, isLoading };
};
