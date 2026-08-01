import api from './api';

export const getMembers = () => api.get('/Members');

export const getMyProfile = () => api.get('/Members/me');

export const createMember = (member) => api.post('/Members', member);
