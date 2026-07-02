def test_register_user(client):
    payload = {
        'full_name': 'New User',
        'email': 'newuser@library.com',
        'password': 'Pass@1234',
        'membership_id': 'MEM-999'
    }
    resp = client.post('/api/auth/register', json=payload)
    assert resp.status_code == 201
    data = resp.get_json()
    assert data['email'] == 'newuser@library.com'
    assert data['role'] == 'Member'

def test_register_duplicate_email(client):
    payload = {
        'full_name': 'Duplicate',
        'email': 'newuser@library.com',
        'password': 'Pass@1234',
        'membership_id': 'MEM-998'
    }
    resp = client.post('/api/auth/register', json=payload)
    assert resp.status_code == 409

def test_login_valid(client):
    payload = {'email': 'newuser@library.com', 'password': 'Pass@1234'}
    resp = client.post('/api/auth/login', json=payload)
    assert resp.status_code == 200
    assert 'access_token' in resp.get_json()

def test_login_invalid_password(client):
    payload = {'email': 'newuser@library.com', 'password': 'wrongpassword'}
    resp = client.post('/api/auth/login', json=payload)
    assert resp.status_code == 401

def test_login_nonexistent_user(client):
    payload = {'email': 'nobody@library.com', 'password': 'irrelevant'}
    resp = client.post('/api/auth/login', json=payload)
    assert resp.status_code == 401

def test_me_requires_auth(client):
    resp = client.get('/api/auth/me')
    assert resp.status_code == 401
