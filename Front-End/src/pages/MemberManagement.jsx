import { useEffect, useState } from 'react';
import { useAuth } from '../context/AuthContext';
import { createMember, getMembers } from '../services/memberService';
import { getBranches } from '../services/branchService';
import api from '../services/api';

const emptyMemberForm = {
  firstName: '',
  lastName: '',
  email: '',
  phone: '',
  membershipExpiryDate: '',
  username: '',
  password: '',
};

const emptyLibrarianForm = {
  firstName: '',
  lastName: '',
  email: '',
  phone: '',
  username: '',
  password: '',
  branchId: '',
};

export default function MemberManagement() {
  const { user } = useAuth();
  const isAdmin = user?.role === 'Admin';

  const [members, setMembers] = useState([]);
  const [branches, setBranches] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  // Member creation state
  const [memberForm, setMemberForm] = useState(emptyMemberForm);
  const [memberFormError, setMemberFormError] = useState('');
  const [memberSubmitting, setMemberSubmitting] = useState(false);
  const [memberMessage, setMemberMessage] = useState('');

  // Librarian creation state (Admin only)
  const [librarianForm, setLibrarianForm] = useState(emptyLibrarianForm);
  const [librarianFormError, setLibrarianFormError] = useState('');
  const [librarianSubmitting, setLibrarianSubmitting] = useState(false);
  const [librarianMessage, setLibrarianMessage] = useState('');

  const loadData = () => {
    setLoading(true);
    setError('');
    
    Promise.all([
      getMembers().then(res => setMembers(res.data)),
      isAdmin ? getBranches().then(res => setBranches(res.data)) : Promise.resolve()
    ])
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  };

  useEffect(loadData, [isAdmin]);

  const extractError = (err) => {
    const data = err.response?.data;
    if (data?.errors && typeof data.errors === 'object') {
      return Object.values(data.errors).flat().join(' ');
    }
    return data?.detail || data?.title || 'Operation failed. Please try again.';
  };

  // Member form handlers
  const handleMemberChange = (event) => {
    setMemberForm({ ...memberForm, [event.target.name]: event.target.value });
  };

  const handleMemberSubmit = async (event) => {
    event.preventDefault();
    setMemberFormError('');
    setMemberMessage('');

    const payload = {
      ...memberForm,
      membershipExpiryDate: new Date(memberForm.membershipExpiryDate).toISOString(),
    };

    setMemberSubmitting(true);
    try {
      await createMember(payload);
      setMemberForm(emptyMemberForm);
      setMemberMessage('Member created successfully.');
      loadData();
    } catch (err) {
      setMemberFormError(extractError(err));
    } finally {
      setMemberSubmitting(false);
    }
  };

  // Librarian form handlers
  const handleLibrarianChange = (event) => {
    setLibrarianForm({ ...librarianForm, [event.target.name]: event.target.value });
  };

  const handleLibrarianSubmit = async (event) => {
    event.preventDefault();
    setLibrarianFormError('');
    setLibrarianMessage('');

    if (!librarianForm.branchId) {
      setLibrarianFormError('Please select a branch.');
      return;
    }

    setLibrarianSubmitting(true);
    try {
      await api.post('/Librarians', librarianForm);
      setLibrarianForm(emptyLibrarianForm);
      setLibrarianMessage('Librarian created successfully.');
    } catch (err) {
      setLibrarianFormError(extractError(err));
    } finally {
      setLibrarianSubmitting(false);
    }
  };

  return (
    <div>
      <h1>Member & Staff Management</h1>

      {/* Create Member Section */}
      <section className="panel">
        <h2>Register New Member</h2>
        <form className="form-grid" onSubmit={handleMemberSubmit}>
          <label className="form-field">
            <span>First Name</span>
            <input name="firstName" value={memberForm.firstName} onChange={handleMemberChange} required />
          </label>
          <label className="form-field">
            <span>Last Name</span>
            <input name="lastName" value={memberForm.lastName} onChange={handleMemberChange} required />
          </label>
          <label className="form-field">
            <span>Email</span>
            <input type="email" name="email" value={memberForm.email} onChange={handleMemberChange} required />
          </label>
          <label className="form-field">
            <span>Phone</span>
            <input name="phone" value={memberForm.phone} onChange={handleMemberChange} required />
          </label>
          <label className="form-field">
            <span>Username</span>
            <input name="username" value={memberForm.username} onChange={handleMemberChange} required autoComplete="off" />
          </label>
          <label className="form-field">
            <span>Password</span>
            <input type="password" name="password" value={memberForm.password} onChange={handleMemberChange} required autoComplete="new-password" placeholder="Min. 6 characters" />
          </label>
          <label className="form-field">
            <span>Membership Expiry</span>
            <input type="date" name="membershipExpiryDate" value={memberForm.membershipExpiryDate} onChange={handleMemberChange} required />
          </label>
          <div className="form-actions">
            <button className="btn btn-primary" type="submit" disabled={memberSubmitting}>
              {memberSubmitting ? 'Creating...' : 'Create Member'}
            </button>
          </div>
        </form>
        {memberFormError && <p className="error">{memberFormError}</p>}
        {memberMessage && <p className="success">{memberMessage}</p>}
      </section>

      {/* Create Librarian Section (Admin only) */}
      {isAdmin && (
        <section className="panel">
          <h2>Register New Librarian</h2>
          <p className="muted" style={{ marginBottom: '1rem' }}>Assign a new librarian to a specific branch.</p>
          <form className="form-grid" onSubmit={handleLibrarianSubmit}>
            <label className="form-field">
              <span>First Name</span>
              <input name="firstName" value={librarianForm.firstName} onChange={handleLibrarianChange} required />
            </label>
            <label className="form-field">
              <span>Last Name</span>
              <input name="lastName" value={librarianForm.lastName} onChange={handleLibrarianChange} required />
            </label>
            <label className="form-field">
              <span>Email</span>
              <input type="email" name="email" value={librarianForm.email} onChange={handleLibrarianChange} required />
            </label>
            <label className="form-field">
              <span>Phone</span>
              <input name="phone" value={librarianForm.phone} onChange={handleLibrarianChange} required />
            </label>
            <label className="form-field">
              <span>Username</span>
              <input name="username" value={librarianForm.username} onChange={handleLibrarianChange} required autoComplete="off" />
            </label>
            <label className="form-field">
              <span>Password</span>
              <input type="password" name="password" value={librarianForm.password} onChange={handleLibrarianChange} required autoComplete="new-password" placeholder="Min. 6 characters" />
            </label>
            <label className="form-field">
              <span>Assign to Branch</span>
              <select name="branchId" value={librarianForm.branchId} onChange={handleLibrarianChange} required>
                <option value="" disabled>-- Select a Branch --</option>
                {branches.map(b => (
                  <option key={b.id} value={b.id}>{b.name}</option>
                ))}
              </select>
            </label>
            <div className="form-actions">
              <button className="btn btn-primary" type="submit" disabled={librarianSubmitting}>
                {librarianSubmitting ? 'Creating...' : 'Create Librarian'}
              </button>
            </div>
          </form>
          {librarianFormError && <p className="error">{librarianFormError}</p>}
          {librarianMessage && <p className="success">{librarianMessage}</p>}
        </section>
      )}

      {/* Members List */}
      <section className="panel">
        <h2>Members Directory</h2>
        {loading && <p className="muted">Loading members...</p>}
        {error && <p className="error">Failed to load members: {error}</p>}
        {!loading && !error && (
          <table className="data-table">
            <thead>
              <tr>
                <th>Name</th>
                <th>Email</th>
                <th>Phone</th>
                <th>Expiry</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {members.map((member) => (
                <tr key={member.id}>
                  <td>
                    {member.firstName} {member.lastName}
                  </td>
                  <td>{member.email}</td>
                  <td>{member.phone}</td>
                  <td>{new Date(member.membershipExpiryDate).toLocaleDateString()}</td>
                  <td>{member.isActive ? 'Active' : 'Inactive'}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
        {!loading && !error && members.length === 0 && <p className="muted">No members found.</p>}
      </section>
    </div>
  );
}
