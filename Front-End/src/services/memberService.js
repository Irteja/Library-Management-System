import api from './api';

export const getMembers = () => api.get('/Members');

export const createMember = (member) => api.post('/Members', member);
