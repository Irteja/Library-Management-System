import { useEffect, useState } from 'react';
import { getReportSummary } from '../services/reportService';

export default function Reports() {
  const [report, setReport] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    getReportSummary()
      .then(({ data }) => setReport(data))
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  }, []);

  if (loading) {
    return <p className="muted">Loading reports...</p>;
  }

  if (error) {
    return <p className="error">Failed to load reports: {error}</p>;
  }

  const maxBorrows = report.topBorrowedBooks.length
    ? Math.max(...report.topBorrowedBooks.map((book) => book.borrowCount))
    : 1;

  return (
    <div>
      <h1>Reports</h1>

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
          <h2 className="section-title">Most Borrowed Books</h2>
          <div className="chart">
            {report.topBorrowedBooks.map((book) => (
              <div className="chart-row" key={book.isbn}>
                <div className="chart-label" title={`${book.title} - ${book.author}`}>
                  {book.title}
                </div>
                <div className="chart-track">
                  <div
                    className="chart-bar"
                    style={{ width: `${(book.borrowCount / maxBorrows) * 100}%` }}
                  >
                    <span className="chart-value">{book.borrowCount}</span>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </>
      )}
    </div>
  );
}
