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

export default api;
