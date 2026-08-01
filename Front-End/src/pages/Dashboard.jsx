import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { getMyProfile } from '../services/memberService';
import { getReportSummary } from '../services/reportService';

const REPORT_ROLES = ['Admin'];

export default function Dashboard() {
  const { user } = useAuth();
  const canViewReports = REPORT_ROLES.includes(user?.role);
  const isMember = user?.role === 'Member';

  const [report, setReport] = useState(null);
  const [loading, setLoading] = useState(canViewReports);
  const [error, setError] = useState('');

  const [profile, setProfile] = useState(null);
  const [profileError, setProfileError] = useState('');

  useEffect(() => {
    if (!canViewReports) return;

    getReportSummary()
      .then(({ data }) => setReport(data))
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  }, [canViewReports]);

  useEffect(() => {
    if (!isMember) return;

    getMyProfile()
      .then(({ data }) => setProfile(data))
      .catch((err) => setProfileError(err.response?.data?.title ?? err.message));
  }, [isMember]);

  return (
    <div>
      <h1>Dashboard</h1>
      <p>
        Welcome, <strong>{user?.username}</strong> ({user?.role})
      </p>

      {isMember && (
        <>
          {profile && (
            <section className="panel">
              <h2>Membership</h2>
              <p>
                <strong>
                  {profile.firstName} {profile.lastName}
                </strong>
              </p>
              <p className="muted">{profile.email}</p>
              <p>
                Expires: {new Date(profile.membershipExpiryDate).toLocaleDateString()}{' '}
                <span className={profile.isActive ? 'success' : 'error'}>
                  {profile.isActive ? '(Active)' : '(Inactive)'}
                </span>
              </p>
            </section>
          )}
          {profileError && <p className="error">{profileError}</p>}

          <div className="stat-grid">
            <div className="stat-card">
              <span className="stat-label">Loans</span>
              <Link to="/my-loans" className="btn btn-primary">
                View My Loans
              </Link>
            </div>
            <div className="stat-card">
              <span className="stat-label">Reservations</span>
              <Link to="/my-reservations" className="btn btn-primary">
                View My Reservations
              </Link>
            </div>
          </div>
        </>
      )}

      {!canViewReports && !isMember && (
        <p className="muted">
          Library statistics are available to Admin accounts only.
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
