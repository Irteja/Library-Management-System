import api from './api';

export const getBranches = (params) => api.get('/Branches', { params });

export const getBranchesCursor = (cursor, limit = 50) => 
  api.get('/Branches/cursor', { params: { cursor, limit } });

export const createBranch = (branch) => api.post('/Branches', branch);
