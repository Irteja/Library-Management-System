import api from './api';

export const getMembers = (params) => api.get('/Members', { params });

export const getMyProfile = () => api.get('/Members/me');

export const createMember = (member) => api.post('/Members', member);

export const updateMember = (id, member) => api.put(`/Members/${id}`, member);
