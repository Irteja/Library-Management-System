import api from './api';

export const getActiveReservations = () => api.get('/Reservations/active');

export const getMyReservations = () => api.get('/Reservations/my');

export const placeReservation = (payload) => api.post('/Reservations', payload);

export const cancelReservation = (id) => api.put(`/Reservations/${id}/cancel`);
