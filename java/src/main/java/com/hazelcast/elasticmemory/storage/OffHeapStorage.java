package com.hazelcast.elasticmemory.storage;

import static com.hazelcast.elasticmemory.util.MathUtil.*;

import java.io.Closeable;
import java.util.concurrent.locks.ReentrantLock;
import java.util.logging.Level;


public class OffHeapStorage extends OffHeapStorageSupport implements Storage {
	
	private final StorageSegment[] segments ;
	
	public OffHeapStorage(int totalSizeInMb, int chunkSizeInKb) {
		this(totalSizeInMb, divideByAndCeil(totalSizeInMb, MAX_SEGMENT_SIZE_IN_MB), chunkSizeInKb);
	}
	
	public OffHeapStorage(int totalSizeInMb, int segmentCount, int chunkSizeInKb) {
		super(totalSizeInMb, segmentCount, chunkSizeInKb);
		
		logger.log(Level.INFO, "Total of " + segmentCount + " segments is going to be initialized...");
		this.segments = new StorageSegment[segmentCount];
		for (int i = 0; i < segmentCount; i++) {
			segments[i] = new StorageSegment(segmentSizeInMb, chunkSizeInKb);
		}
	}
	
	private StorageSegment getSegment(int hash) {
		return segments[(hash == Integer.MIN_VALUE) ? 0 : Math.abs(hash) % segmentCount];
	}

	public EntryRef put(int hash, byte[] value) {
		return getSegment(hash).put(value);
	}

	public byte[] get(int hash, EntryRef entry) {
		return getSegment(hash).get(entry);
	}

	public void remove(int hash, EntryRef entry) {
		getSegment(hash).remove(entry);
	}
	
	public void destroy() {
		destroy(segments);
	}
	
	private class StorageSegment extends ReentrantLock implements Closeable {
		private BufferSegment buffer;

		StorageSegment(int totalSizeInMb, int chunkSizeInKb) {
			super();
			buffer = new BufferSegment(totalSizeInMb, chunkSizeInKb);
		}

		EntryRef put(final byte[] value) {
			lock();
			try {
				return buffer.put(value);
			} finally {
				unlock();
			}
		}

		byte[] get(final EntryRef entry) {
			lock();
			try {
				return buffer.get(entry);
			} finally {
				unlock();
			}
		}
		
		void remove(final EntryRef entry) {
			lock();
			try {
				buffer.remove(entry);
			} finally {
				unlock();
			}
		}
		
		public void close() {
			if(buffer != null) {
				buffer.destroy();
				buffer = null;
			}
		}
	}
}