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
  const [searchQuery, setSearchQuery] = useState('');
  const [searchInput, setSearchInput] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const itemsPerPage = 10;

  const [form, setForm] = useState({ memberId: '', bookId: '', branchId: '' });
  const [formError, setFormError] = useState('');
  const [formMessage, setFormMessage] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [showForm, setShowForm] = useState(false);

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [cancellingId, setCancellingId] = useState(null);

  const loadOptions = async () => {
    const [memberRes, bookRes, branchRes] = await Promise.all([
      getMembers({ size: 1000 }),
      getBooks({ size: 1000 }),
      getBranches({ size: 1000 }),
    ]);
    setMembers(memberRes.data.items || []);
    setBooks(bookRes.data.items || []);
    setBranches(branchRes.data.items || []);
  };

  const loadQueue = () => {
    setLoading(true);
    setError('');
    getActiveReservations({ search: searchQuery, page: currentPage, size: itemsPerPage })
      .then(({ data }) => {
        setReservations(data.items || []);
        setTotalPages(data.totalPages || 1);
      })
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    loadOptions().catch((err) => setFormError(err.message));
  }, []);

  useEffect(() => {
    loadQueue();
  }, [currentPage, searchQuery]);

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
      setShowForm(false);
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



  return (
    <div>
      <div className="page-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h1>Reservation Queue</h1>
        {!showForm && (
          <button className="btn btn-primary" onClick={() => { setForm({ memberId: '', bookId: '', branchId: '' }); setFormError(''); setFormMessage(''); setShowForm(true); }}>
            + Place a Reservation
          </button>
        )}
      </div>

      {showForm && (
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
              {books.map((book) => {
                const isAvailable = book.availableCopies > 0;
                return (
                  <option key={book.id} value={book.id} disabled={isAvailable}>
                    {book.title} - {book.author} {isAvailable ? '(Available to borrow)' : ''}
                  </option>
                );
              })}
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
            <button className="btn btn-outline" type="button" onClick={() => setShowForm(false)} disabled={submitting}>
              Cancel
            </button>
            <button className="btn btn-primary" type="submit" disabled={submitting}>
              {submitting ? 'Placing...' : 'Place Reservation'}
            </button>
          </div>
        </form>
        {formError && <p className="error">{formError}</p>}
        {formMessage && <p className="success">{formMessage}</p>}
      </section>
      )}

      <section className="panel">
        <h2>Active Reservations</h2>
        {loading && <p className="muted">Loading reservations...</p>}
        {error && <p className="error">Failed to load reservations: {error}</p>}
        {!loading && !error && (
          <>
            <form onSubmit={(e) => {
              e.preventDefault();
              setSearchQuery(searchInput);
              setCurrentPage(1);
            }} style={{ marginBottom: '1rem', display: 'flex', gap: '0.5rem' }}>
              <input
                type="text"
                placeholder="Search by member name or book title..."
                value={searchInput}
                onChange={(e) => setSearchInput(e.target.value)}
                style={{ padding: '0.5rem', width: '100%', maxWidth: '400px' }}
              />
              <button type="submit" className="btn btn-primary">Search</button>
            </form>
            {reservations.length > 0 ? (
              <>
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
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginTop: '1rem' }}>
                  <button
                    className="btn btn-outline"
                    onClick={() => setCurrentPage(p => Math.max(1, p - 1))}
                    disabled={currentPage === 1}
                  >
                    Previous
                  </button>
                  <span>Page {currentPage} of {totalPages}</span>
                  <button
                    className="btn btn-outline"
                    onClick={() => setCurrentPage(p => Math.min(totalPages, p + 1))}
                    disabled={currentPage === totalPages}
                  >
                    Next
                  </button>
                </div>
              </>
            ) : (
              <p className="muted">No active reservations found.</p>
            )}
          </>
        )}
      </section>
    </div>
  );
}
