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

  // Search and Pagination
  const [searchTerm, setSearchTerm] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const itemsPerPage = 10;

  const filteredBranches = branches.filter((branch) => {
    const term = searchTerm.toLowerCase();
    return (
      (branch.name && branch.name.toLowerCase().includes(term)) ||
      (branch.address && branch.address.toLowerCase().includes(term)) ||
      (branch.email && branch.email.toLowerCase().includes(term))
    );
  });

  const totalPages = Math.ceil(filteredBranches.length / itemsPerPage) || 1;
  const startIndex = (currentPage - 1) * itemsPerPage;
  const currentBranches = filteredBranches.slice(startIndex, startIndex + itemsPerPage);

  const loadBranches = () => {
    setLoading(true);
    setError('');
    getBranches()
      .then(({ data }) => setBranches(data))
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  };

  useEffect(loadBranches, []);

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
      loadBranches();
    } catch (err) {
      setFormError(err.response?.data?.title ?? 'Failed to create branch.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div>
      <h1>Branches</h1>

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
            <button className="btn btn-primary" type="submit" disabled={submitting}>
              {submitting ? 'Creating...' : 'Create Branch'}
            </button>
          </div>
        </form>
        {formError && <p className="error">{formError}</p>}
        {message && <p className="success">{message}</p>}
      </section>

      <section className="panel">
        <h2>Branches</h2>

        {!loading && !error && branches.length > 0 && (
          <div style={{ marginBottom: '1rem' }}>
            <input
              type="text"
              placeholder="Search by name, address, or email..."
              value={searchTerm}
              onChange={(e) => {
                setSearchTerm(e.target.value);
                setCurrentPage(1);
              }}
              style={{ padding: '0.5rem', width: '100%', maxWidth: '300px', borderRadius: '4px', border: '1px solid #ccc' }}
            />
          </div>
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
                {currentBranches.length > 0 ? (
                  currentBranches.map((branch) => (
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
