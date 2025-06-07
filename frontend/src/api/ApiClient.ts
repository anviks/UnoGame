import axios, { AxiosError, type AxiosRequestConfig } from 'axios';
import { useToast } from 'vue-toastification';

type CustomAxiosConfig = AxiosRequestConfig & { errorMessage?: string, getFullResponse?: boolean }

const axiosInstance = axios.create({
  baseURL: `${import.meta.env.VITE_BACKEND_URL}/api`,
  withCredentials: true,
});

export class ApiClient {
  /**
   * Generic method to handle API requests
   * @param config - Axios request configuration
   */
  static async request(config: CustomAxiosConfig): Promise<any> {
    const { errorMessage = null, getFullResponse = false, ...axiosConfig } = config;
    const toast = useToast();
    try {
      const response = await axiosInstance(axiosConfig);
      return getFullResponse ? response : response.data;
    } catch (error: any | AxiosError) {
      if (axios.isAxiosError(error)) {
        if (error.response?.data?.constructor === Array) {
          let delay = 0;
          for (const err of error.response?.data) {
            let errString = err.message;
            if (errorMessage !== null) errString = errorMessage + '\n' + errString;
            setTimeout(() => {
              toast.error(errString);
            }, delay);
            delay += 500;
          }
        }
        if (!error.response?.data?.length && errorMessage !== null) {
          toast.error(errorMessage);
        }
      } else if (errorMessage !== null) {
        toast.error(errorMessage);
      }

      throw error;
    }
  }

  /**
   * Generic method to handle GET requests
   * @param url - The URL to send the GET request to
   * @param [config] - Axios request configuration
   */
  static async get(url: string, config: CustomAxiosConfig = {}): Promise<any> {
    return await this.request({ method: 'GET', url, ...config });
  }

  /**
   * Generic method to handle POST requests
   * @param  url - The URL to send the POST request to
   * @param [data] - The data to send in the POST request
   * @param [config] - Axios request configuration
   */
  static async post(url: string, data: object | null = null, config: CustomAxiosConfig = {}): Promise<any> {
    return await this.request({ method: 'POST', url, data, ...config });
  }

  /**
   * Generic method to handle PUT requests
   * @param url - The URL to send the PUT request to
   * @param [data] - The data to send in the PUT request
   * @param [config] - Axios request configuration
   */
  static async put(url: string, data: object | null = null, config: CustomAxiosConfig = {}): Promise<any> {
    return await this.request({ method: 'PUT', url, data, ...config });
  }

  /**
   * Generic method to handle DELETE requests
   * @param url - The URL to send the DELETE request to
   * @param [config] - Axios request configuration
   */
  static async delete(url: string, config: CustomAxiosConfig = {}): Promise<any> {
    return await this.request({ method: 'DELETE', url, ...config });
  }
}
