import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import MainLayout from './components/MainLayout';
import ProtectedRoute from './components/ProtectedRoute';
import { AuthProvider, useAuth } from './context/AuthContext';
import Books from './pages/Books';
import Branches from './pages/Branches';
import BorrowReturn from './pages/BorrowReturn';
import Dashboard from './pages/Dashboard';
import Login from './pages/Login';
import Register from './pages/Register';
import MemberManagement from './pages/MemberManagement';
import MyLoans from './pages/MyLoans';
import MyReservations from './pages/MyReservations';
import NotFound from './pages/NotFound';
import Reports from './pages/Reports';
import ReservationQueue from './pages/ReservationQueue';
import StaffManagement from './pages/StaffManagement';
import Unauthorized from './pages/Unauthorized';

const DEFAULT_PATHS = {
  Admin: '/dashboard',
  Librarian: '/books',
  Member: '/dashboard',
};

function DefaultRoute() {
  const { role } = useAuth();
  return <Navigate to={DEFAULT_PATHS[role] ?? '/login'} replace />;
}

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />

          <Route
            element={
              <ProtectedRoute>
                <MainLayout />
              </ProtectedRoute>
            }
          >
            <Route
              path="/dashboard"
              element={
                <ProtectedRoute allowedRoles={['Admin', 'Member']}>
                  <Dashboard />
                </ProtectedRoute>
              }
            />
            <Route path="/books" element={<Books />} />
            <Route
              path="/my-loans"
              element={
                <ProtectedRoute allowedRoles={['Member']}>
                  <MyLoans />
                </ProtectedRoute>
              }
            />
            <Route
              path="/my-reservations"
              element={
                <ProtectedRoute allowedRoles={['Member']}>
                  <MyReservations />
                </ProtectedRoute>
              }
            />
            <Route
              path="/members"
              element={
                <ProtectedRoute allowedRoles={['Admin', 'Librarian']}>
                  <MemberManagement />
                </ProtectedRoute>
              }
            />
            <Route
              path="/branches"
              element={
                <ProtectedRoute allowedRoles={['Admin']}>
                  <Branches />
                </ProtectedRoute>
              }
            />
            <Route
              path="/borrow-return"
              element={
                <ProtectedRoute allowedRoles={['Admin', 'Librarian']}>
                  <BorrowReturn />
                </ProtectedRoute>
              }
            />
            <Route
              path="/reservations"
              element={
                <ProtectedRoute allowedRoles={['Admin', 'Librarian']}>
                  <ReservationQueue />
                </ProtectedRoute>
              }
            />
            <Route
              path="/reports"
              element={
                <ProtectedRoute allowedRoles={['Admin']}>
                  <Reports />
                </ProtectedRoute>
              }
            />
            <Route
              path="/staff"
              element={
                <ProtectedRoute allowedRoles={['Admin']}>
                  <StaffManagement />
                </ProtectedRoute>
              }
            />
            <Route path="/" element={<DefaultRoute />} />
          </Route>

          <Route path="/unauthorized" element={<Unauthorized />} />
          <Route path="*" element={<NotFound />} />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  );
}
