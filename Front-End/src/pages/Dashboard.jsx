import { useAuth } from '../context/AuthContext';

export default function Dashboard() {
  const { user } = useAuth();

  return (
    <div>
      <h1>Dashboard</h1>
      <p>
        Welcome, <strong>{user?.username}</strong> ({user?.role})
      </p>
    </div>
  );
}
