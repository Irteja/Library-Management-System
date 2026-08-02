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

  const loadData = () => {
    setLoading(true);
    setError('');
    
    Promise.all([
      getLibrarians().then(res => setLibrarians(res.data)),
      getBranches().then(res => setBranches(res.data))
    ])
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    loadData();
  }, []);

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
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
        <h1 style={{ margin: 0 }}>Staff Management</h1>
        {!showForm && (
          <button className="btn btn-primary" onClick={() => setShowForm(true)}>
            + Register New Librarian
          </button>
        )}
      </div>

      {/* Create Librarian Form */}
      {showForm && (
        <section className="panel" style={{ marginBottom: '2rem' }}>
          <h2>Register New Librarian</h2>
          <p className="muted" style={{ marginBottom: '1rem' }}>Assign a new librarian to a specific branch.</p>
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
        {loading && <p className="muted">Loading staff...</p>}
        {error && <p className="error">Failed to load staff: {error}</p>}
        {!loading && !error && (
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
              {librarians.map((lib) => (
                <tr key={lib.id}>
                  <td>
                    {lib.firstName} {lib.lastName}
                  </td>
                  <td>{lib.username}</td>
                  <td>{lib.email}</td>
                  <td>{lib.phone}</td>
                  <td>{lib.branchName}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
        {!loading && !error && librarians.length === 0 && <p className="muted">No staff found.</p>}
      </section>
    </div>
  );
}
