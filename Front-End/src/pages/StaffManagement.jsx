import { useEffect, useState } from 'react';
import { getLibrarians, createLibrarian } from '../services/librarianService';
import { getBranches } from '../services/branchService';

const emptyLibrarianForm = {
  firstName: '',
  lastName: '',
  email: '',
  phone: '',
  username: '',
  password: '',
  branchId: '',
};

export default function StaffManagement() {
  const [librarians, setLibrarians] = useState([]);
  const [branches, setBranches] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  // Form state
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState(emptyLibrarianForm);
  const [formError, setFormError] = useState('');
  const [submitting, setSubmitting] = useState(false);

  // Search and Pagination
  const [searchQuery, setSearchQuery] = useState('');
  const [searchInput, setSearchInput] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const itemsPerPage = 10;

  const loadData = () => {
    setLoading(true);
    setError('');
    
    Promise.all([
      getLibrarians({ search: searchQuery, page: currentPage, size: itemsPerPage }).then(res => {
        setLibrarians(res.data.items || []);
        setTotalPages(res.data.totalPages || 1);
      }),
      getBranches({ size: 1000 }).then(res => setBranches(res.data.items || []))
    ])
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    loadData();
  }, [currentPage, searchQuery]);

  const extractError = (err) => {
    const data = err.response?.data;
    if (data?.errors && typeof data.errors === 'object') {
      return Object.values(data.errors).flat().join(' ');
    }
    return data?.detail || data?.title || 'Operation failed. Please try again.';
  };

  const handleChange = (event) => {
    setForm({ ...form, [event.target.name]: event.target.value });
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    setFormError('');

    if (!form.branchId) {
      setFormError('Please select a branch.');
      return;
    }

    setSubmitting(true);
    try {
      await createLibrarian(form);
      setForm(emptyLibrarianForm);
      setShowForm(false);
      loadData();
    } catch (err) {
      setFormError(extractError(err));
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div>
      <div className="page-header">
        <h1>Staff Management</h1>
        {!showForm && (
          <button className="btn btn-primary" onClick={() => setShowForm(true)}>
            + Register New Librarian
          </button>
        )}
      </div>

      {/* Create Librarian Form */}
      {showForm && (
        <section className="panel">
          <h2>Register New Librarian</h2>
          <p className="muted">Assign a new librarian to a specific branch.</p>
          <form className="form-grid" onSubmit={handleSubmit}>
            <label className="form-field">
              <span>First Name</span>
              <input name="firstName" value={form.firstName} onChange={handleChange} required />
            </label>
            <label className="form-field">
              <span>Last Name</span>
              <input name="lastName" value={form.lastName} onChange={handleChange} required />
            </label>
            <label className="form-field">
              <span>Email</span>
              <input type="email" name="email" value={form.email} onChange={handleChange} required />
            </label>
            <label className="form-field">
              <span>Phone</span>
              <input name="phone" value={form.phone} onChange={handleChange} required />
            </label>
            <label className="form-field">
              <span>Username</span>
              <input name="username" value={form.username} onChange={handleChange} required autoComplete="off" />
            </label>
            <label className="form-field">
              <span>Password</span>
              <input type="password" name="password" value={form.password} onChange={handleChange} required autoComplete="new-password" placeholder="Min. 6 characters" />
            </label>
            <label className="form-field">
              <span>Assign to Branch</span>
              <select name="branchId" value={form.branchId} onChange={handleChange} required>
                <option value="" disabled>-- Select a Branch --</option>
                {branches.map(b => (
                  <option key={b.id} value={b.id}>{b.name}</option>
                ))}
              </select>
            </label>
            <div className="form-actions">
              <button className="btn btn-outline" type="button" onClick={() => setShowForm(false)} disabled={submitting}>
                Cancel
              </button>
              <button className="btn btn-primary" type="submit" disabled={submitting}>
                {submitting ? 'Creating...' : 'Create Librarian'}
              </button>
            </div>
          </form>
          {formError && <p className="error">{formError}</p>}
        </section>
      )}

      {/* Librarians List */}
      <section className="panel">
        <h2>Librarians Directory</h2>

        {!loading && !error && (
          <form onSubmit={(e) => {
            e.preventDefault();
            setSearchQuery(searchInput);
            setCurrentPage(1);
          }} style={{ marginBottom: '1rem', display: 'flex', gap: '0.5rem' }}>
            <input
              type="text"
              placeholder="Search by name, username, or branch..."
              value={searchInput}
              onChange={(e) => setSearchInput(e.target.value)}
              style={{ padding: '0.5rem', width: '100%', maxWidth: '300px', borderRadius: '4px', border: '1px solid #ccc' }}
            />
            <button type="submit" className="btn btn-primary">Search</button>
          </form>
        )}

        {loading && <p className="muted">Loading staff...</p>}
        {error && <p className="error">Failed to load staff: {error}</p>}
        {!loading && !error && librarians.length > 0 && (
          <>
            <table className="data-table">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Username</th>
                  <th>Email</th>
                  <th>Phone</th>
                  <th>Branch</th>
                </tr>
              </thead>
              <tbody>
                {librarians.length > 0 ? (
                  librarians.map((lib) => (
                    <tr key={lib.id}>
                      <td>
                        {lib.firstName} {lib.lastName}
                      </td>
                      <td>{lib.username}</td>
                      <td>{lib.email}</td>
                      <td>{lib.phone}</td>
                      <td>{lib.branchName}</td>
                    </tr>
                  ))
                ) : (
                  <tr>
                    <td colSpan="5" className="muted" style={{ textAlign: 'center', padding: '1rem' }}>
                      No matching staff found.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>

            {totalPages > 1 && (
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginTop: '1rem' }}>
                <button 
                  className="btn btn-outline" 
                  disabled={currentPage === 1}
                  onClick={() => setCurrentPage(p => p - 1)}
                >
                  Previous
                </button>
                <span>Page {currentPage} of {totalPages}</span>
                <button 
                  className="btn btn-outline" 
                  disabled={currentPage === totalPages}
                  onClick={() => setCurrentPage(p => p + 1)}
                >
                  Next
                </button>
              </div>
            )}
          </>
        )}
        {!loading && !error && librarians.length === 0 && <p className="muted">No staff found.</p>}
      </section>
    </div>
  );
}
