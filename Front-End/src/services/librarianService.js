import api from './api';

export const getLibrarians = () => api.get('/Librarians');

export const createLibrarian = (librarian) => api.post('/Librarians', librarian);
