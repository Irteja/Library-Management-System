import api from './api';

export const getBranches = (params) => api.get('/Branches', { params });

export const createBranch = (branch) => api.post('/Branches', branch);
