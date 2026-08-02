import api from './api';

export const getActiveLoans = (params) => api.get('/Loans/active', { params });

export const getMyLoans = (params) => api.get('/Loans/my', { params });

export const borrowBook = (payload) => api.post('/Loans/borrow', payload);

export const returnBook = (loanId) => api.post('/Loans/return', { loanId });
