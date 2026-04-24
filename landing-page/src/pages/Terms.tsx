
import { motion } from 'framer-motion';
import { FileText } from 'lucide-react';
import { Link } from 'react-router-dom';

export default function Terms() {
  return (
    <div className="min-h-screen bg-slate-950 text-slate-100 py-32 px-6">
      <div className="max-w-3xl mx-auto">
        <Link to="/" className="text-cyan-400 hover:text-cyan-300 font-bold mb-8 inline-block">← Back to Home</Link>
        <motion.div 
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          className="glass p-10 rounded-[40px] border-white/10"
        >
          <div className="w-16 h-16 bg-purple-500/10 rounded-2xl flex items-center justify-center mb-8">
            <FileText className="w-8 h-8 text-purple-400" />
          </div>
          <h1 className="text-4xl font-black mb-6">Terms of Service</h1>
          <div className="space-y-6 text-slate-400 leading-relaxed">
            <p>Last updated: April 2026</p>
            <section>
              <h2 className="text-xl font-bold text-white mb-3">1. Acceptance of Terms</h2>
              <p>By using WordConnect Bangla, you agree to comply with these terms.</p>
            </section>
            <section>
              <h2 className="text-xl font-bold text-white mb-3">2. License to Use</h2>
              <p>We grant you a personal, non-exclusive license to play the game for personal entertainment.</p>
            </section>
            <section>
              <h2 className="text-xl font-bold text-white mb-3">3. Prohibited Conduct</h2>
              <p>You may not modify, reverse engineer, or exploit the game for commercial purposes.</p>
            </section>
          </div>
        </motion.div>
      </div>
    </div>
  );
}
