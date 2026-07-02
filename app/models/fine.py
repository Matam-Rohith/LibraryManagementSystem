from app import db
from datetime import datetime

class Fine(db.Model):
    __tablename__ = 'fines'
    id = db.Column(db.Integer, primary_key=True)
    borrow_record_id = db.Column(db.Integer, db.ForeignKey('borrow_records.id'), nullable=False)
    amount = db.Column(db.Float, nullable=False)
    is_paid = db.Column(db.Boolean, default=False)
    created_at = db.Column(db.DateTime, default=datetime.utcnow)

    def to_dict(self):
        return {
            'id': self.id,
            'borrow_record_id': self.borrow_record_id,
            'amount': self.amount,
            'is_paid': self.is_paid,
            'created_at': self.created_at.isoformat()
        }
