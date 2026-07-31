import api from './api';

export const getBooks = () => api.get('/Books');

export const getBookById = (id) => api.get(`/Books/${id}`);

export const createBook = (book) => api.post('/Books', book);

export const updateBook = (id, book) => api.put(`/Books/${id}`, { id, ...book });

export const deleteBook = (id) => api.delete(`/Books/${id}`);
