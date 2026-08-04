import { useEffect, useState } from 'react';
import DateInput from '../components/DateInput';
import { getBooks } from '../services/bookService';
import { getBranchesCursor } from '../services/branchService';

const loadAllBranches = async () => {
  let allBranches = [];
  let currentCursor = null;
  let hasNext = true;
  
  while (hasNext) {
    const res = await getBranchesCursor(currentCursor, 100);
    allBranches = [...allBranches, ...res.data.items];
    currentCursor = res.data.nextCursor;
    hasNext = res.data.hasNextPage;
  }
  return allBranches;
};
import { borrowBook, getActiveLoans, returnBook } from '../services/loanService';
import { getMembers } from '../services/memberService';

export default function BorrowReturn() {
  const [members, setMembers] = useState([]);
  const [books, setBooks] = useState([]);
  const [branches, setBranches] = useState([]);
  const [loans, setLoans] = useState([]);
  const [searchQuery, setSearchQuery] = useState('');
  const [searchInput, setSearchInput] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const itemsPerPage = 10;

  const [form, setForm] = useState({ memberId: '', bookId: '', branchId: '', dueDate: null });
  const [formError, setFormError] = useState('');
  const [formMessage, setFormMessage] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [showForm, setShowForm] = useState(false);

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [returningId, setReturningId] = useState(null);

  const loadOptions = async () => {
    const [memberRes, bookRes, branchRes] = await Promise.all([
      getMembers({ size: 1000 }),
      getBooks({ size: 1000 }),
      loadAllBranches(),
    ]);
    setMembers(memberRes.data.items || []);
    setBooks(bookRes.data.items || []);
    setBranches(branchRes);
  };

  const loadLoans = () => {
    setLoading(true);
    setError('');
    getActiveLoans({ search: searchQuery, page: currentPage, size: itemsPerPage })
      .then(({ data }) => {
        setLoans(data.items || []);
        setTotalPages(data.totalPages || 1);
      })
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    loadOptions().catch((err) => setFormError(err.message));
  }, []);

  useEffect(() => {
    loadLoans();
  }, [currentPage, searchQuery]);

  const handleChange = (event) => {
    setForm({ ...form, [event.target.name]: event.target.value });
  };

  const handleBorrow = async (event) => {
    event.preventDefault();
    setFormError('');
    setFormMessage('');
    setSubmitting(true);
    try {
      const payload = {
        ...form,
        dueDate: form.dueDate?.toISOString() ?? undefined,
      };
      await borrowBook(payload);
      setFormMessage('Book issued successfully.');
      setForm({ memberId: '', bookId: '', branchId: '', dueDate: null });
      setShowForm(false);
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
      <div className="page-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h1>Borrow &amp; Return (Updated)</h1>
        {!showForm && (
          <button className="btn btn-primary" onClick={() => { setForm({ memberId: '', bookId: '', branchId: '', dueDate: null }); setFormError(''); setFormMessage(''); setShowForm(true); }}>
            + Issue a Loan
          </button>
        )}
      </div>

      {showForm && (
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

          <div className="form-field">
            <span>Return Date</span>
            <DateInput
              selected={form.dueDate}
              onChange={(date) => setForm({ ...form, dueDate: date })}
              minDate={new Date()}
              placeholderText="Select return date..."
            />
          </div>

          <div className="form-actions">
            <button className="btn btn-outline" type="button" onClick={() => setShowForm(false)} disabled={submitting}>
              Cancel
            </button>
            <button className="btn btn-primary" type="submit" disabled={submitting}>
              {submitting ? 'Issuing...' : 'Issue Book'}
            </button>
          </div>
        </form>
        {formError && <p className="error">{formError}</p>}
        {formMessage && <p className="success">{formMessage}</p>}
      </section>
      )}

      <section className="panel">
        <h2>Active Loans</h2>
        {loading && <p className="muted">Loading loans...</p>}
        {error && <p className="error">Failed to load loans: {error}</p>}
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
            {loans.length > 0 ? (
              <>
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
              <p className="muted">No active loans found.</p>
            )}
          </>
        )}
      </section>
    </div>
  );
}
