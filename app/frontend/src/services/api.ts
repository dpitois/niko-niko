import axios from 'axios';

const api = axios.create({
  baseURL: '/api',
});

// Add an interceptor to include the JWT token in all requests
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('jwt_token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

let isRefreshing = false;
let failedQueue: { resolve: (token: string) => void; reject: (error: unknown) => void }[] = [];

// Callback for logout when refresh fails
let logoutHandler: (() => void) | null = null;
export const setLogoutHandler = (handler: () => void) => {
  logoutHandler = handler;
};

const processQueue = (error: unknown, token: string | null = null) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error);
    } else if (token) {
      prom.resolve(token);
    }
  });

  failedQueue = [];
};

api.interceptors.response.use(
  (response) => {
    return response;
  },
  async (error) => {
    const originalRequest = error.config;

    if (error.response?.status === 401 && !originalRequest._retry) {
      if (isRefreshing) {
        return new Promise(function (resolve, reject) {
          failedQueue.push({ resolve, reject });
        })
          .then((token) => {
            originalRequest.headers.Authorization = 'Bearer ' + token;
            return api(originalRequest);
          })
          .catch((err) => {
            return Promise.reject(err);
          });
      }

      originalRequest._retry = true;
      isRefreshing = true;

      try {
        const response = await api.post('/auth/refresh-token');
        const { token } = response.data;
        localStorage.setItem('jwt_token', token);
        api.defaults.headers.common.Authorization = 'Bearer ' + token;
        processQueue(null, token);
        isRefreshing = false;
        originalRequest.headers.Authorization = 'Bearer ' + token;
        return api(originalRequest);
      } catch (err) {
        processQueue(err, null);
        isRefreshing = false;
        // If refresh fails, let the caller handle it (usually redirects to login)
        if (logoutHandler) {
          logoutHandler();
        }
        return Promise.reject(err);
      }
    }

    return Promise.reject(error);
  },
);

export default api;
