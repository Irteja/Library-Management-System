import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

const NAV_ITEMS = [
  { label: 'Dashboard', path: '/dashboard', roles: ['Admin', 'Librarian', 'Member'] },
  { label: 'Books', path: '/books', roles: ['Admin', 'Librarian', 'Member'] },
  { label: 'Borrow & Return', path: '/borrow-return', roles: ['Admin', 'Librarian'] },
  { label: 'Members', path: '/members', roles: ['Admin', 'Librarian'] },
  { label: 'My Loans', path: '/my-loans', roles: ['Member'] },
];

export default function MainLayout() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const visibleItems = NAV_ITEMS.filter((item) => item.roles.includes(user?.role));

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
          {visibleItems.map((item) => (
            <NavLink key={item.path} to={item.path} className={navLinkClass} end={item.path === '/'}>
              {item.label}
            </NavLink>
          ))}
        </nav>
        <div className="app-user">
          {user && (
            <>
              <span className="user-info">
                {user.username} <span className="user-role">({user.role})</span>
              </span>
              <button className="btn btn-outline" onClick={handleLogout}>
                Logout
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
