import apiClient from './apiClient';

export const login = (credentials) => apiClient.post('/Auth/login', credentials);
