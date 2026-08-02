import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

const NAV_ITEMS = [
  {
    label: 'Dashboard',
    path: '/dashboard',
    roles: ['Admin', 'Member'],
    labelByRole: { Member: 'My Dashboard' },
  },
  {
    label: 'Books',
    path: '/books',
    roles: ['Admin', 'Librarian', 'Member'],
    labelByRole: { Member: 'Book Catalog' },
  },
  {
    label: 'My Loans',
    path: '/my-loans',
    roles: ['Member'],
  },
  {
    label: 'My Reservations',
    path: '/my-reservations',
    roles: ['Member'],
  },
  { label: 'Borrow & Return', path: '/borrow-return', roles: ['Admin', 'Librarian'] },
  { label: 'Reservations', path: '/reservations', roles: ['Admin', 'Librarian'] },
  { label: 'Members', path: '/members', roles: ['Admin', 'Librarian'] },
  { label: 'Branches', path: '/branches', roles: ['Admin'] },
  { label: 'Staff', path: '/staff', roles: ['Admin'] },
  { label: 'Reports', path: '/reports', roles: ['Admin'] },
];

export default function MainLayout() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const role = user?.role;

  const visibleItems = NAV_ITEMS.filter((item) => item.roles.includes(role));

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
            <NavLink key={item.path} to={item.path} className={navLinkClass}>
              {item.labelByRole?.[role] ?? item.label}
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
