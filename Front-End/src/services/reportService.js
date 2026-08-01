import api from './api';

export const getReportSummary = () => api.get('/Reports/summary');
