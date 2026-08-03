import { useEffect, useState } from 'react';
import { createBranch, getBranches } from '../services/branchService';

const emptyForm = {
  name: '',
  address: '',
  phone: '',
  email: '',
};

export default function Branches() {
  const [branches, setBranches] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [form, setForm] = useState(emptyForm);
  const [formError, setFormError] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [message, setMessage] = useState('');
  const [showForm, setShowForm] = useState(false);

  // Search and Pagination
  const [searchQuery, setSearchQuery] = useState('');
  const [searchInput, setSearchInput] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const itemsPerPage = 10;

  const loadBranches = () => {
    setLoading(true);
    setError('');
    getBranches({ search: searchQuery, page: currentPage, size: itemsPerPage })
      .then(({ data }) => {
        setBranches(data.items || []);
        setTotalPages(data.totalPages || 1);
      })
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    loadBranches();
  }, [currentPage, searchQuery]);

  const handleChange = (event) => {
    setForm({ ...form, [event.target.name]: event.target.value });
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    setFormError('');
    setMessage('');
    setSubmitting(true);
    try {
      await createBranch(form);
      setForm(emptyForm);
      setMessage('Branch created successfully.');
      setShowForm(false);
      loadBranches();
    } catch (err) {
      setFormError(err.response?.data?.title ?? 'Failed to create branch.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div>
      <div className="page-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h1>Branches</h1>
        {!showForm && (
          <button className="btn btn-primary" onClick={() => { setForm(emptyForm); setFormError(''); setMessage(''); setShowForm(true); }}>
            + Add New Branch
          </button>
        )}
      </div>

      {showForm && (
        <section className="panel">
          <h2>Add New Branch</h2>
        <form className="form-grid" onSubmit={handleSubmit}>
          <label className="form-field">
            <span>Name</span>
            <input name="name" value={form.name} onChange={handleChange} required />
          </label>
          <label className="form-field">
            <span>Address</span>
            <input name="address" value={form.address} onChange={handleChange} required />
          </label>
          <label className="form-field">
            <span>Phone</span>
            <input name="phone" value={form.phone} onChange={handleChange} required />
          </label>
          <label className="form-field">
            <span>Email</span>
            <input type="email" name="email" value={form.email} onChange={handleChange} required />
          </label>
          <div className="form-actions">
            <button className="btn btn-outline" type="button" onClick={() => setShowForm(false)} disabled={submitting}>
              Cancel
            </button>
            <button className="btn btn-primary" type="submit" disabled={submitting}>
              {submitting ? 'Creating...' : 'Create Branch'}
            </button>
          </div>
        </form>
        {formError && <p className="error">{formError}</p>}
        {message && <p className="success">{message}</p>}
      </section>
      )}

      <section className="panel">
        <h2>Branches</h2>

        {!loading && !error && (
          <form onSubmit={(e) => {
            e.preventDefault();
            setSearchQuery(searchInput);
            setCurrentPage(1);
          }} style={{ marginBottom: '1rem', display: 'flex', gap: '0.5rem' }}>
            <input
              type="text"
              placeholder="Search by name, address, or email..."
              value={searchInput}
              onChange={(e) => setSearchInput(e.target.value)}
              style={{ padding: '0.5rem', width: '100%', maxWidth: '300px', borderRadius: '4px', border: '1px solid #ccc' }}
            />
            <button type="submit" className="btn btn-primary">Search</button>
          </form>
        )}

        {loading && <p className="muted">Loading branches...</p>}
        {error && <p className="error">Failed to load branches: {error}</p>}
        {!loading && !error && branches.length > 0 && (
          <>
            <table className="data-table">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Address</th>
                  <th>Phone</th>
                  <th>Email</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {branches.length > 0 ? (
                  branches.map((branch) => (
                    <tr key={branch.id}>
                      <td>{branch.name}</td>
                      <td>{branch.address}</td>
                      <td>{branch.phone}</td>
                      <td>{branch.email}</td>
                      <td>{branch.isActive ? 'Active' : 'Inactive'}</td>
                    </tr>
                  ))
                ) : (
                  <tr>
                    <td colSpan="5" className="muted" style={{ textAlign: 'center', padding: '1rem' }}>
                      No matching branches found.
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
        {!loading && !error && branches.length === 0 && <p className="muted">No branches found.</p>}
      </section>
    </div>
  );
}
