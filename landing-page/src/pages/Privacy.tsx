
import { motion } from 'framer-motion';
import { Shield } from 'lucide-react';
import { Link } from 'react-router-dom';

export default function Privacy() {
  return (
    <div className="min-h-screen bg-slate-950 text-slate-100 py-32 px-6">
      <div className="max-w-3xl mx-auto">
        <Link to="/" className="text-cyan-400 hover:text-cyan-300 font-bold mb-8 inline-block">← Back to Home</Link>
        <motion.div 
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          className="glass p-10 rounded-[40px] border-white/10"
        >
          <div className="w-16 h-16 bg-cyan-500/10 rounded-2xl flex items-center justify-center mb-8">
            <Shield className="w-8 h-8 text-cyan-400" />
          </div>
          <h1 className="text-4xl font-black mb-6">Privacy Policy</h1>
          <div className="space-y-6 text-slate-400 leading-relaxed">
            <p>Last updated: April 2026</p>
            <section>
              <h2 className="text-xl font-bold text-white mb-3">1. Information We Collect</h2>
              <p>WordConnect Bangla does not collect any personal information. We may collect anonymous usage data to improve game performance.</p>
            </section>
            <section>
              <h2 className="text-xl font-bold text-white mb-3">2. How We Use Information</h2>
              <p>Data collected is used solely for game optimization and providing better user experiences.</p>
            </section>
            <section>
              <h2 className="text-xl font-bold text-white mb-3">3. Data Security</h2>
              <p>We implement strict security measures to protect any data we handle.</p>
            </section>
          </div>
        </motion.div>
      </div>
    </div>
  );
}
