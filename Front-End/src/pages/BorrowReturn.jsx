import { useEffect, useState } from 'react';
import { getBooks } from '../services/bookService';
import { getBranches } from '../services/branchService';
import { borrowBook, getActiveLoans, returnBook } from '../services/loanService';
import { getMembers } from '../services/memberService';

export default function BorrowReturn() {
  const [members, setMembers] = useState([]);
  const [books, setBooks] = useState([]);
  const [branches, setBranches] = useState([]);
  const [loans, setLoans] = useState([]);

  const [form, setForm] = useState({ memberId: '', bookId: '', branchId: '' });
  const [formError, setFormError] = useState('');
  const [formMessage, setFormMessage] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [returningId, setReturningId] = useState(null);

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

  const loadLoans = () => {
    setLoading(true);
    setError('');
    getActiveLoans()
      .then(({ data }) => setLoans(data))
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    loadOptions().catch((err) => setFormError(err.message));
    loadLoans();
  }, []);

  const handleChange = (event) => {
    setForm({ ...form, [event.target.name]: event.target.value });
  };

  const handleBorrow = async (event) => {
    event.preventDefault();
    setFormError('');
    setFormMessage('');
    setSubmitting(true);
    try {
      await borrowBook(form);
      setFormMessage('Book issued successfully.');
      setForm({ memberId: '', bookId: '', branchId: '' });
      loadLoans();
    } catch (err) {
      setFormError(err.response?.data?.title ?? 'Failed to issue loan.');
    } finally {
      setSubmitting(false);
    }
  };

  const handleReturn = async (loanId) => {
    setReturningId(loanId);
    setFormMessage('');
    setFormError('');
    try {
      await returnBook(loanId);
      loadLoans();
    } catch (err) {
      setError(err.response?.data?.title ?? 'Failed to return book.');
    } finally {
      setReturningId(null);
    }
  };

  const availableBooks = books.filter((book) => book.availableCopies > 0);
  const activeMembers = members.filter((member) => member.isActive);
  const activeBranches = branches.filter((branch) => branch.isActive);

  return (
    <div>
      <h1>Borrow &amp; Return</h1>

      <section className="panel">
        <h2>Issue a Loan</h2>
        <form className="form-grid" onSubmit={handleBorrow}>
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
              {availableBooks.map((book) => (
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
              {submitting ? 'Issuing...' : 'Issue Book'}
            </button>
          </div>
        </form>
        {formError && <p className="error">{formError}</p>}
        {formMessage && <p className="success">{formMessage}</p>}
      </section>

      <section className="panel">
        <h2>Active Loans</h2>
        {loading && <p className="muted">Loading loans...</p>}
        {error && <p className="error">Failed to load loans: {error}</p>}
        {!loading && !error && (
          <table className="data-table">
            <thead>
              <tr>
                <th>Book</th>
                <th>Member</th>
                <th>Borrowed</th>
                <th>Due</th>
                <th>Status</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {loans.map((loan) => (
                <tr key={loan.id}>
                  <td>
                    {loan.bookTitle} <span className="muted">({loan.bookAuthor})</span>
                  </td>
                  <td>{loan.memberName}</td>
                  <td>{new Date(loan.loanDate).toLocaleDateString()}</td>
                  <td>{new Date(loan.dueDate).toLocaleDateString()}</td>
                  <td>{loan.status}</td>
                  <td>
                    <button
                      className="btn btn-outline"
                      onClick={() => handleReturn(loan.id)}
                      disabled={returningId === loan.id}
                    >
                      {returningId === loan.id ? 'Returning...' : 'Return'}
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
        {!loading && !error && loans.length === 0 && <p className="muted">No active loans.</p>}
      </section>
    </div>
  );
}
