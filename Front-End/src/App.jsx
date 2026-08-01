import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import MainLayout from './components/MainLayout';
import ProtectedRoute from './components/ProtectedRoute';
import { AuthProvider } from './context/AuthContext';
import Books from './pages/Books';
import BorrowReturn from './pages/BorrowReturn';
import Dashboard from './pages/Dashboard';
import Login from './pages/Login';
import MemberManagement from './pages/MemberManagement';
import MyLoans from './pages/MyLoans';
import NotFound from './pages/NotFound';
import Reports from './pages/Reports';
import ReservationQueue from './pages/ReservationQueue';

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route path="/login" element={<Login />} />

          <Route
            element={
              <ProtectedRoute>
                <MainLayout />
              </ProtectedRoute>
            }
          >
            <Route path="/dashboard" element={<Dashboard />} />
            <Route path="/books" element={<Books />} />
            <Route
              path="/my-loans"
              element={
                <ProtectedRoute roles={['Member']}>
                  <MyLoans />
                </ProtectedRoute>
              }
            />
            <Route
              path="/members"
              element={
                <ProtectedRoute roles={['Admin', 'Librarian']}>
                  <MemberManagement />
                </ProtectedRoute>
              }
            />
            <Route
              path="/borrow-return"
              element={
                <ProtectedRoute roles={['Admin', 'Librarian']}>
                  <BorrowReturn />
                </ProtectedRoute>
              }
            />
            <Route
              path="/reservations"
              element={
                <ProtectedRoute roles={['Admin', 'Librarian']}>
                  <ReservationQueue />
                </ProtectedRoute>
              }
            />
            <Route
              path="/reports"
              element={
                <ProtectedRoute roles={['Admin', 'Librarian']}>
                  <Reports />
                </ProtectedRoute>
              }
            />
            <Route path="/" element={<Navigate to="/dashboard" replace />} />
          </Route>

          <Route path="*" element={<NotFound />} />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  );
}
