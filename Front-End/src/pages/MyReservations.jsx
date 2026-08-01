import { useEffect, useState } from 'react';
import { cancelReservation, getMyReservations } from '../services/reservationService';

export default function MyReservations() {
  const [reservations, setReservations] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [cancellingId, setCancellingId] = useState(null);
  const [message, setMessage] = useState('');

  const loadReservations = () => {
    setLoading(true);
    setError('');
    getMyReservations()
      .then(({ data }) => setReservations(data))
      .catch((err) => setError(err.response?.data?.title ?? err.message))
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    loadReservations();
  }, []);

  const handleCancel = async (id) => {
    setCancellingId(id);
    setMessage('');
    setError('');
    try {
      await cancelReservation(id);
      setMessage('Reservation cancelled.');
      loadReservations();
    } catch (err) {
      setError(err.response?.data?.title ?? 'Failed to cancel reservation.');
    } finally {
      setCancellingId(null);
    }
  };

  return (
    <div>
      <h1>My Reservations</h1>

      {message && <p className="success">{message}</p>}
      {loading && <p className="muted">Loading reservations...</p>}
      {error && <p className="error">Failed to load reservations: {error}</p>}

      {!loading && !error && (
        <table className="data-table">
          <thead>
            <tr>
              <th>#</th>
              <th>Book</th>
              <th>Author</th>
              <th>Reserved</th>
              <th>Expires</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {reservations.map((reservation) => (
              <tr key={reservation.id}>
                <td>{reservation.queuePosition}</td>
                <td>{reservation.bookTitle}</td>
                <td>{reservation.bookAuthor}</td>
                <td>{new Date(reservation.reservedAt).toLocaleDateString()}</td>
                <td>{new Date(reservation.expiresAt).toLocaleDateString()}</td>
                <td>
                  <button
                    className="btn btn-outline"
                    onClick={() => handleCancel(reservation.id)}
                    disabled={cancellingId === reservation.id}
                  >
                    {cancellingId === reservation.id ? 'Cancelling...' : 'Cancel'}
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
      {!loading && !error && reservations.length === 0 && (
        <p className="muted">You have no pending reservations.</p>
      )}
    </div>
  );
}
