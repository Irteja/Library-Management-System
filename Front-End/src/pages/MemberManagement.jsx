import { useEffect, useState } from 'react';
import DateInput from '../components/DateInput';
import { createMember, getMembers, updateMember } from '../services/memberService';

const emptyMemberForm = {
  firstName: '',
  lastName: '',
  email: '',
  phone: '',
  membershipExpiryDate: null,
  username: '',
  password: '',
};

export default function MemberManagement() {
  const [members, setMembers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  // Member creation state
  const [memberForm, setMemberForm] = useState(emptyMemberForm);
  const [memberFormError, setMemberFormError] = useState('');
  const [memberSubmitting, setMemberSubmitting] = useState(false);
  const [memberMessage, setMemberMessage] = useState('');
  const [showForm, setShowForm] = useState(false);

  // Search and Pagination
  const [searchQuery, setSearchQuery] = useState('');
  const [searchInput, setSearchInput] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const itemsPerPage = 10;

  // Extension state
  const [extendingMember, setExtendingMember] = useState(null);
  const [newExpiryDate, setNewExpiryDate] = useState(null);
  const [isExtending, setIsExtending] = useState(false);

  const loadData = () => {
    setLoading(true);
    setError('');
    
    getMembers({ search: searchQuery, page: currentPage, size: itemsPerPage })
      .then(res => {
        setMembers(res.data.items || []);
        setTotalPages(res.data.totalPages || 1);
      })
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
      membershipExpiryDate: memberForm.membershipExpiryDate?.toISOString(),
    };

    setMemberSubmitting(true);
    try {
      await createMember(payload);
      setMemberForm(emptyMemberForm);
      setMemberMessage('Member created successfully.');
      setShowForm(false);
      loadData();
    } catch (err) {
      setMemberFormError(extractError(err));
    } finally {
      setMemberSubmitting(false);
    }
  };

  const handleExtendSubmit = async (event) => {
    event.preventDefault();
    setIsExtending(true);
    try {
      const payload = {
        id: extendingMember.id,
        firstName: extendingMember.firstName,
        lastName: extendingMember.lastName,
        email: extendingMember.email,
        phone: extendingMember.phone,
        isActive: extendingMember.isActive,
        membershipExpiryDate: newExpiryDate?.toISOString(),
      };
      await updateMember(extendingMember.id, payload);
      setExtendingMember(null);
      setNewExpiryDate(null);
      loadData();
    } catch (err) {
      alert(extractError(err));
    } finally {
      setIsExtending(false);
    }
  };

  return (
    <div>
      <div className="page-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h1>Member Management</h1>
        {!showForm && (
          <button className="btn btn-primary" onClick={() => { setMemberForm(emptyMemberForm); setMemberFormError(''); setMemberMessage(''); setShowForm(true); }}>
            + Register New Member
          </button>
        )}
      </div>

      {/* Create Member Section */}
      {showForm && (
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
          <div className="form-field">
            <span>Membership Expiry</span>
            <DateInput
              selected={memberForm.membershipExpiryDate}
              onChange={(date) => handleMemberChange({ target: { name: 'membershipExpiryDate', value: date } })}
              minDate={new Date()}
              placeholderText="Select expiry date..."
            />
          </div>
          <div className="form-actions">
            <button className="btn btn-outline" type="button" onClick={() => setShowForm(false)} disabled={memberSubmitting}>
              Cancel
            </button>
            <button className="btn btn-primary" type="submit" disabled={memberSubmitting}>
              {memberSubmitting ? 'Creating...' : 'Create Member'}
            </button>
          </div>
        </form>
        {memberFormError && <p className="error">{memberFormError}</p>}
        {memberMessage && <p className="success">{memberMessage}</p>}
      </section>
      )}

      {/* Members List */}
      <section className="panel">
        <h2>Members Directory</h2>

        {!loading && !error && (
          <>
            <form onSubmit={(e) => {
              e.preventDefault();
              setSearchQuery(searchInput);
              setCurrentPage(1);
            }} style={{ marginBottom: '1rem', display: 'flex', gap: '0.5rem' }}>
              <input
                type="text"
                placeholder="Search by name or email..."
                value={searchInput}
                onChange={(e) => setSearchInput(e.target.value)}
                style={{ padding: '0.5rem', width: '100%', maxWidth: '300px', borderRadius: '4px', border: '1px solid #ccc' }}
              />
              <button type="submit" className="btn btn-primary">Search</button>
            </form>

            {extendingMember && (
              <div className="panel" style={{ marginBottom: '1rem', border: '1px solid var(--primary-color)' }}>
                <h3>Extend Membership for {extendingMember.firstName} {extendingMember.lastName}</h3>
                <form onSubmit={handleExtendSubmit} style={{ display: 'flex', gap: '1rem', alignItems: 'flex-end', flexWrap: 'wrap' }}>
                  <div className="form-field">
                    <span>New Expiry Date</span>
                    <DateInput
                      selected={newExpiryDate}
                      onChange={(date) => setNewExpiryDate(date)}
                      minDate={new Date()}
                      placeholderText="Select new expiry date..."
                    />
                  </div>
                  <div className="form-actions">
                    <button type="submit" className="btn btn-primary" disabled={isExtending}>Confirm</button>
                    <button type="button" className="btn btn-outline" onClick={() => setExtendingMember(null)}>Cancel</button>
                  </div>
                </form>
              </div>
            )}
          </>
        )}

        {loading && <p className="muted">Loading members...</p>}
        {error && <p className="error">Failed to load members: {error}</p>}
        {!loading && !error && members.length > 0 && (
          <>
            <table className="data-table">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Email</th>
                  <th>Phone</th>
                  <th>Expiry</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {members.length > 0 ? (
                  members.map((member) => (
                    <tr key={member.id}>
                      <td>
                        {member.firstName} {member.lastName}
                      </td>
                      <td>{member.email}</td>
                      <td>{member.phone}</td>
                      <td>{new Date(member.membershipExpiryDate).toLocaleDateString()}</td>
                      <td>{member.isActive ? 'Active' : 'Inactive'}</td>
                      <td>
                        <button 
                          className="btn btn-outline"
                          style={{ padding: '0.25rem 0.5rem', fontSize: '0.875rem' }}
                          disabled={!member.isActive}
                          title={!member.isActive ? "Only active members can be extended" : "Extend membership"}
                          onClick={() => {
                            setExtendingMember(member);
                            setNewExpiryDate(member.membershipExpiryDate.split('T')[0]);
                          }}
                        >
                          Extend
                        </button>
                      </td>
                    </tr>
                  ))
                ) : (
                  <tr>
                    <td colSpan="6" className="muted" style={{ textAlign: 'center', padding: '1rem' }}>
                      No matching members found.
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
        {!loading && !error && members.length === 0 && <p className="muted">No members found.</p>}
      </section>
    </div>
  );
}
