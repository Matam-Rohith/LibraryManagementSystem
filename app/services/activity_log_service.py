from datetime import datetime
from app import mongo_db

def log_activity(action, user_id, user_email, role,
                 entity_type=None, entity_id=None, details=None, ip_address=None):
    try:
        mongo_db['activity_logs'].insert_one({
            'action': action,
            'user_id': user_id,
            'user_email': user_email,
            'role': role,
            'entity_type': entity_type,
            'entity_id': entity_id,
            'details': details,
            'ip_address': ip_address,
            'timestamp': datetime.utcnow()
        })
    except Exception:
        pass
