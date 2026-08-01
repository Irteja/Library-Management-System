import { useEffect, useState } from 'react';
import { getBooks } from '../services/bookService';
import { getBranches } from '../services/branchService';
import { getMembers } from '../services/memberService';
import {
  cancelReservation,
  getActiveReservations,
  placeReservation,
} from '../services/reservationService';

export default function ReservationQueue() {
  const [members, setMembers] = useState([]);
  const [books, setBooks] = useState([]);
  const [branches, setBranches] = useState([]);
  const [reservations, setReservations] = useState([]);

  const [form, setForm] = useState({ memberId: '', bookId: '', branchId: '' });
  const [formError, setFormError] = useState('');
  const [formMessage, setFormMessage] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [cancellingId, setCancellingId] = useState(null);

  const loadOptions = async () => {
    const [memberRes, bookRes, branchRes] = await Promise.all([
      getMembers(),
      getBooks(),
      getBranches(),
    ]);
    setMembers(memberRes.data);
    setBooks(bookRes.data);
    setBranches(branchRes.data);
  };

  const loadQueue = () => {
    setLoading(true);
    setError('');
    getActiveReservations()
      .then(({ data }) => setReservations(data))
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    loadOptions().catch((err) => setFormError(err.message));
    loadQueue();
  }, []);

  const handleChange = (event) => {
    setForm({ ...form, [event.target.name]: event.target.value });
  };

  const handleReserve = async (event) => {
    event.preventDefault();
    setFormError('');
    setFormMessage('');
    setSubmitting(true);
    try {
      await placeReservation(form);
      setFormMessage('Reservation placed successfully.');
      setForm({ memberId: '', bookId: '', branchId: '' });
      loadQueue();
    } catch (err) {
      setFormError(err.response?.data?.title ?? 'Failed to place reservation.');
    } finally {
      setSubmitting(false);
    }
  };

  const handleCancel = async (id) => {
    setCancellingId(id);
    setFormMessage('');
    setFormError('');
    try {
      await cancelReservation(id);
      loadQueue();
    } catch (err) {
      setError(err.response?.data?.title ?? 'Failed to cancel reservation.');
    } finally {
      setCancellingId(null);
    }
  };

  const activeMembers = members.filter((member) => member.isActive);
  const activeBranches = branches.filter((branch) => branch.isActive);
  const outOfStockBooks = books.filter((book) => book.availableCopies === 0);

  return (
    <div>
      <h1>Reservation Queue</h1>

      <section className="panel">
        <h2>Place a Reservation</h2>
        <form className="form-grid" onSubmit={handleReserve}>
          <label className="form-field">
            <span>Member</span>
            <select name="memberId" value={form.memberId} onChange={handleChange} required>
              <option value="">Select member...</option>
              {activeMembers.map((member) => (
                <option key={member.id} value={member.id}>
                  {member.firstName} {member.lastName} ({member.email})
                </option>
              ))}
            </select>
          </label>

          <label className="form-field">
            <span>Book</span>
            <select name="bookId" value={form.bookId} onChange={handleChange} required>
              <option value="">Select book...</option>
              {outOfStockBooks.map((book) => (
                <option key={book.id} value={book.id}>
                  {book.title} - {book.author}
                </option>
              ))}
            </select>
          </label>

          <label className="form-field">
            <span>Branch</span>
            <select name="branchId" value={form.branchId} onChange={handleChange} required>
              <option value="">Select branch...</option>
              {activeBranches.map((branch) => (
                <option key={branch.id} value={branch.id}>
                  {branch.name}
                </option>
              ))}
            </select>
          </label>

          <div className="form-actions">
            <button className="btn btn-primary" type="submit" disabled={submitting}>
              {submitting ? 'Placing...' : 'Place Reservation'}
            </button>
          </div>
        </form>
        {formError && <p className="error">{formError}</p>}
        {formMessage && <p className="success">{formMessage}</p>}
      </section>

      <section className="panel">
        <h2>Active Reservations</h2>
        {loading && <p className="muted">Loading reservations...</p>}
        {error && <p className="error">Failed to load reservations: {error}</p>}
        {!loading && !error && (
          <table className="data-table">
            <thead>
              <tr>
                <th>#</th>
                <th>Book</th>
                <th>Member</th>
                <th>Reserved</th>
                <th>Expires</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {reservations.map((reservation) => (
                <tr key={reservation.id}>
                  <td>{reservation.queuePosition}</td>
                  <td>
                    {reservation.bookTitle} <span className="muted">({reservation.bookAuthor})</span>
                  </td>
                  <td>{reservation.memberName}</td>
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
          <p className="muted">No active reservations.</p>
        )}
      </section>
    </div>
  );
}
