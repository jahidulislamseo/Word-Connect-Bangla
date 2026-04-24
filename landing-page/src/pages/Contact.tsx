
import { motion } from 'framer-motion';
import { Mail, MessageSquare, MapPin } from 'lucide-react';
import { Link } from 'react-router-dom';

export default function Contact() {
  return (
    <div className="min-h-screen bg-slate-950 text-slate-100 py-32 px-6">
      <div className="max-w-3xl mx-auto">
        <Link to="/" className="text-cyan-400 hover:text-cyan-300 font-bold mb-8 inline-block">← Back to Home</Link>
        <motion.div 
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          className="glass p-10 rounded-[40px] border-white/10"
        >
          <div className="w-16 h-16 bg-green-500/10 rounded-2xl flex items-center justify-center mb-8">
            <Mail className="w-8 h-8 text-green-400" />
          </div>
          <h1 className="text-4xl font-black mb-6">Contact Us</h1>
          <p className="text-slate-400 mb-10">Have questions or feedback? We'd love to hear from you!</p>

          <form className="space-y-6 mb-12">
            <div className="grid md:grid-cols-2 gap-6">
              <input type="text" placeholder="Full Name" className="w-full bg-white/5 border border-white/10 rounded-2xl py-4 px-6 focus:outline-none focus:border-cyan-500/50 transition-all" />
              <input type="email" placeholder="Email Address" className="w-full bg-white/5 border border-white/10 rounded-2xl py-4 px-6 focus:outline-none focus:border-cyan-500/50 transition-all" />
            </div>
            <textarea placeholder="Your Message" rows={5} className="w-full bg-white/5 border border-white/10 rounded-2xl py-4 px-6 focus:outline-none focus:border-cyan-500/50 transition-all"></textarea>
            <button className="w-full py-4 bg-cyan-500 text-slate-950 font-black rounded-2xl hover:bg-cyan-400 transition-all shadow-lg shadow-cyan-500/20">
              SEND MESSAGE
            </button>
          </form>

          <div className="grid md:grid-cols-2 gap-8 border-t border-white/10 pt-10">
            <div className="flex gap-4">
              <MessageSquare className="w-6 h-6 text-cyan-400 shrink-0" />
              <div>
                <p className="font-bold text-white">Email Support</p>
                <p className="text-slate-400 text-sm">support@wordconnectbangla.com</p>
              </div>
            </div>
            <div className="flex gap-4">
              <MapPin className="w-6 h-6 text-cyan-400 shrink-0" />
              <div>
                <p className="font-bold text-white">Location</p>
                <p className="text-slate-400 text-sm">Dhaka, Bangladesh</p>
              </div>
            </div>
          </div>
        </motion.div>
      </div>
    </div>
  );
}
