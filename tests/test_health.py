def test_health_check(client):
    resp = client.get('/health')
    assert resp.status_code in [200, 503]
    data = resp.get_json()
    assert 'status' in data
    assert 'postgres' in data
    assert 'mongodb' in data
