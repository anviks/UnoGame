import axios, { AxiosError, type AxiosRequestConfig } from 'axios';
import { useToast } from 'vue-toastification';

export type ApiRequestOptions = AxiosRequestConfig & {
  successMessage?: string;
  errorMessage?: string;
  showErrorToast?: boolean;
};

export type ApiResponse<T> =
  | {
      data: T;
      error: null;
      success: true;
    }
  | {
      data: null;
      error: AxiosError;
      success: false;
    };

const axiosInstance = axios.create({
  baseURL: `${import.meta.env.VITE_BACKEND_URL}/api`,
  withCredentials: true,
});

/**
 * Generic method to handle API requests
 * @param config - Axios request configuration
 * @param toast
 */
export async function apiRequest<T>(
  config: ApiRequestOptions,
  toast: ReturnType<typeof useToast>
): Promise<ApiResponse<T>> {
  const {
    successMessage,
    errorMessage,
    showErrorToast = true,
    ...axiosConfig
  } = config;

  try {
    const data = await axiosInstance<T>(axiosConfig);

    if (successMessage) {
      toast.success(successMessage, {
        timeout: 3000,
      });
    }

    return {
      data: data.data,
      error: null,
      success: true,
    };
  } catch (err) {
    const error = err as AxiosError;

    if (showErrorToast) {
      const baseMessage = error.message;

      toast.error(
        errorMessage
          ? errorMessage.replace('{error}', baseMessage)
          : `Request failed: ${baseMessage}`,
        {
          timeout: 5000,
        }
      );
    }

    return {
      data: null,
      error,
      success: false,
    };
  }
}
