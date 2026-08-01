import { useEffect, useState } from 'react';
import { getMyLoans } from '../services/loanService';

export default function MyLoans() {
  const [loans, setLoans] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    getMyLoans()
      .then(({ data }) => setLoans(data))
      .catch((err) => setError(err.response?.data?.title ?? err.message))
      .finally(() => setLoading(false));
  }, []);

  return (
    <div>
      <h1>My Loans</h1>

      {loading && <p className="muted">Loading loans...</p>}
      {error && <p className="error">Failed to load loans: {error}</p>}

      {!loading && !error && (
        <table className="data-table">
          <thead>
            <tr>
              <th>Book</th>
              <th>Author</th>
              <th>Loan Date</th>
              <th>Due Date</th>
              <th>Returned</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            {loans.map((loan) => (
              <tr key={loan.id}>
                <td>{loan.bookTitle}</td>
                <td>{loan.bookAuthor}</td>
                <td>{new Date(loan.loanDate).toLocaleDateString()}</td>
                <td>{new Date(loan.dueDate).toLocaleDateString()}</td>
                <td>
                  {loan.returnDate ? new Date(loan.returnDate).toLocaleDateString() : '—'}
                </td>
                <td>
                  <span className={`badge badge-${loan.status?.toLowerCase()}`}>{loan.status}</span>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
      {!loading && !error && loans.length === 0 && (
        <p className="muted">You have no loans yet.</p>
      )}
    </div>
  );
}
