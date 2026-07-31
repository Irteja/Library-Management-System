import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function AppLayout() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login', { replace: true });
  };

  const navLinkClass = ({ isActive }) => (isActive ? 'nav-link active' : 'nav-link');

  return (
    <div className="app">
      <header className="app-header">
        <span className="app-brand">Library MS</span>
        <nav className="app-nav">
          <NavLink to="/dashboard" className={navLinkClass}>
            Dashboard
          </NavLink>
          <NavLink to="/books" className={navLinkClass}>
            Books
          </NavLink>
        </nav>
        <div className="app-user">
          {user && (
            <>
              <span className="user-info">
                {user.username} <span className="user-role">({user.role})</span>
              </span>
              <button className="btn btn-outline" onClick={handleLogout}>
                Sign Out
              </button>
            </>
          )}
        </div>
      </header>
      <main className="app-main">
        <Outlet />
      </main>
    </div>
  );
}
