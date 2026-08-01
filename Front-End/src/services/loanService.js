import api from './api';

export const getActiveLoans = () => api.get('/Loans/active');

export const borrowBook = (payload) => api.post('/Loans/borrow', payload);

export const returnBook = (loanId) => api.post('/Loans/return', { loanId });
