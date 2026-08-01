import api from './api';

export const getBranches = () => api.get('/Branches');

export const createBranch = (branch) => api.post('/Branches', branch);
