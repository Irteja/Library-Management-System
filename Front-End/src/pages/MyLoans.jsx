import { useAuth } from '../context/AuthContext';

export default function MyLoans() {
  const { user } = useAuth();

  return (
    <div>
      <h1>My Loans</h1>
      <p>
        Loan history for <strong>{user?.username}</strong> will be listed here.
      </p>
      <p className="muted">
        This page requires linking your account to a library member record before
        live data can be displayed.
      </p>
    </div>
  );
}
