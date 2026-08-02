import api from './api';

export const getLibrarians = (params) => api.get('/Librarians', { params });

export const createLibrarian = (librarian) => api.post('/Librarians', librarian);
