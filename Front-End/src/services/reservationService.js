import api from './api';

export const getActiveReservations = (params) => api.get('/Reservations/active', { params });

export const getMyReservations = (params) => api.get('/Reservations/my', { params });

export const placeReservation = (payload) => api.post('/Reservations', payload);

export const cancelReservation = (id) => api.put(`/Reservations/${id}/cancel`);
