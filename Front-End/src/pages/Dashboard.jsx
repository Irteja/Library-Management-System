import { useEffect, useState } from 'react';
import { useAuth } from '../context/AuthContext';
import { getReportSummary } from '../services/reportService';

const REPORT_ROLES = ['Admin', 'Librarian'];

export default function Dashboard() {
  const { user } = useAuth();
  const canViewReports = REPORT_ROLES.includes(user?.role);

  const [report, setReport] = useState(null);
  const [loading, setLoading] = useState(canViewReports);
  const [error, setError] = useState('');

  useEffect(() => {
    if (!canViewReports) return;

    getReportSummary()
      .then(({ data }) => setReport(data))
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  }, [canViewReports]);

  return (
    <div>
      <h1>Dashboard</h1>
      <p>
        Welcome, <strong>{user?.username}</strong> ({user?.role})
      </p>

      {!canViewReports && (
        <p className="muted">
          Library statistics are available to Admin and Librarian accounts only.
        </p>
      )}

      {loading && <p className="muted">Loading statistics...</p>}

      {error && <p className="error">Failed to load statistics: {error}</p>}

      {report && (
        <>
          <div className="stat-grid">
            <div className="stat-card">
              <span className="stat-value">{report.totalBooks}</span>
              <span className="stat-label">Total Books</span>
            </div>
            <div className="stat-card">
              <span className="stat-value">{report.totalMembers}</span>
              <span className="stat-label">Total Members</span>
            </div>
            <div className="stat-card">
              <span className="stat-value">{report.activeLoansCount}</span>
              <span className="stat-label">Active Loans</span>
            </div>
            <div className="stat-card stat-card-warn">
              <span className="stat-value">{report.overdueLoansCount}</span>
              <span className="stat-label">Overdue Loans</span>
            </div>
          </div>

          {report.topBorrowedBooks.length > 0 && (
            <>
              <h2 className="section-title">Top Borrowed Books</h2>
              <table className="data-table">
                <thead>
                  <tr>
                    <th>Title</th>
                    <th>Author</th>
                    <th>ISBN</th>
                    <th>Times Borrowed</th>
                  </tr>
                </thead>
                <tbody>
                  {report.topBorrowedBooks.map((book) => (
                    <tr key={book.isbn}>
                      <td>{book.title}</td>
                      <td>{book.author}</td>
                      <td>{book.isbn}</td>
                      <td>{book.borrowCount}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </>
          )}
        </>
      )}
    </div>
  );
}
