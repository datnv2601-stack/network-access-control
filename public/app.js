* {
  box-sizing: border-box;
  margin: 0;
  padding: 0;
  font-family: Arial, sans-serif;
}

body {
  background: #f4f7fb;
  color: #1f2937;
}

.container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 24px;
}

.topbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
}

.topbar h1 {
  font-size: 2rem;
  margin-bottom: 4px;
}

.topbar p {
  color: #6b7280;
}

.primary-btn {
  background: #2563eb;
  color: white;
  border: none;
  border-radius: 8px;
  padding: 10px 18px;
  cursor: pointer;
  font-weight: 600;
}

.primary-btn:hover {
  background: #1d4ed8;
}

.cards {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
  gap: 16px;
  margin-bottom: 24px;
}

.card {
  background: white;
  border-radius: 12px;
  padding: 18px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
}

.card .label {
  color: #6b7280;
  font-size: 0.85rem;
}

.card .value {
  margin-top: 10px;
  font-size: 2rem;
  font-weight: bold;
}

.panel {
  background: white;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
  padding: 20px;
  margin-bottom: 24px;
}

.panel-header {
  margin-bottom: 16px;
}

.form-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
  gap: 12px;
}

.small-grid {
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
}

input, select, textarea, button {
  width: 100%;
  padding: 10px 12px;
  border: 1px solid #d1d5db;
  border-radius: 8px;
  font-size: 0.95rem;
}

textarea {
  min-height: 80px;
  resize: vertical;
}

.table-wrap {
  overflow-x: auto;
}

table {
  width: 100%;
  border-collapse: collapse;
}

th, td {
  padding: 12px 10px;
  text-align: left;
  border-bottom: 1px solid #e5e7eb;
}

.status-badge {
  display: inline-block;
  padding: 6px 10px;
  border-radius: 999px;
  font-size: 0.8rem;
  font-weight: 700;
  text-transform: capitalize;
}

.status-approved {
  background: #dcfce7;
  color: #166534;
}

.status-pending {
  background: #fef3c7;
  color: #92400e;
}

.status-blocked {
  background: #fee2e2;
  color: #991b1b;
}

.status-quarantined {
  background: #e0e7ff;
  color: #3730a3;
}

.action-btn {
  border: none;
  background: #e5e7eb;
  color: #111827;
  border-radius: 6px;
  padding: 6px 10px;
  cursor: pointer;
}

.policy-list {
  margin-top: 20px;
  display: grid;
  gap: 12px;
}

.policy-item {
  background: #f8fafc;
  border: 1px solid #e5e7eb;
  border-radius: 10px;
  padding: 12px 14px;
}

.policy-item strong {
  display: block;
  margin-bottom: 6px;
}

@media (max-width: 700px) {
  .topbar {
    flex-direction: column;
    align-items: flex-start;
    gap: 10px;
  }
}
